using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Auth;

public sealed record LoginRequest(
    string Email,
    string Password
);

public sealed record LoginResponse(TokenPair Tokens);

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class LoginHandler
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<Entities.Admin> _passwordHasher;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        AppDbContext context,
        IJwtService jwtService,
        PasswordHasher<Entities.Admin> passwordHasher,
        ILogger<LoginHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email, ct);

        if (admin == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(
            null!,
            admin.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Failed login attempt for account: {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var tokens = await _jwtService.GenerateAdminTokenPairAsync(admin.Id, admin.Email);

        var refreshToken = new Entities.AdminRefreshToken
        {
            Id = Guid.NewGuid(),
            AdminId = admin.Id,
            Token = tokens.RefreshToken,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.AdminRefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Account logged in successfully: {Email}", admin.Email);

        return new LoginResponse(tokens);
    }
}

public static class Login
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/login", async (
                LoginRequest request,
                LoginHandler handler,
                CancellationToken ct) =>
            {
                var validator = new LoginValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<LoginResponse>.Ok(response));
            })
            .WithName("Login")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
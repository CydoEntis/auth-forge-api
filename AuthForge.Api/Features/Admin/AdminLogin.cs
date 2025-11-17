using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminLoginRequest(
    string Email,
    string Password
);

public sealed record AdminLoginResponse(TokenPair Tokens);

public class AdminLoginValidator : AbstractValidator<AdminLoginRequest>
{
    public AdminLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class AdminLoginHandler
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<Entities.Admin> _passwordHasher;
    private readonly ILogger<AdminLoginHandler> _logger;

    public AdminLoginHandler(
        AppDbContext context,
        IJwtService jwtService,
        PasswordHasher<Entities.Admin> passwordHasher,
        ILogger<AdminLoginHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AdminLoginResponse> HandleAsync(AdminLoginRequest request, CancellationToken ct)
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
            _logger.LogWarning("Failed login attempt for admin: {Email}", request.Email);
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

        _logger.LogInformation("Admin logged in successfully: {Email}", admin.Email);

        return new AdminLoginResponse(tokens);
    }
}

public static class AdminLogin
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/login", async (
                AdminLoginRequest request,
                AdminLoginHandler handler,
                CancellationToken ct) =>
            {
                var validator = new AdminLoginValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<AdminLoginResponse>.Ok(response));
            })
            .WithName("AdminLogin")
            .WithTags("Admin")
            .AllowAnonymous();
    }
}
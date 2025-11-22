using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserLoginRequest(
    string Email,
    string Password
);

public sealed record UserLoginResponse(
    UserDto User,
    TokenPair Tokens
);

public sealed class UserLoginValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public sealed class UserLoginHandler
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<Entities.User> _passwordHasher;
    private readonly ILogger<UserLoginHandler> _logger;

    public UserLoginHandler(
        AppDbContext context,
        IJwtService jwtService,
        PasswordHasher<Entities.User> passwordHasher,
        ILogger<UserLoginHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserLoginResponse> HandleAsync(
        Guid applicationId,
        UserLoginRequest request,
        CancellationToken ct)
    {
        var app = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (app == null || !app.IsActive)
        {
            _logger.LogWarning(
                "Login attempt for invalid application: {AppId}",
                applicationId);
            throw new UnauthorizedException("Invalid email or password");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.ApplicationId == applicationId
                && u.Email == request.Email, ct);

        if (user == null)
        {
            _logger.LogWarning(
                "Login attempt with non-existent email {Email} for app {AppId}",
                request.Email,
                applicationId);
            throw new UnauthorizedException("Invalid email or password");
        }

        if (user.LockedOutUntil.HasValue && user.LockedOutUntil > DateTime.UtcNow)
        {
            var minutesRemaining = (int)(user.LockedOutUntil.Value - DateTime.UtcNow).TotalMinutes;
            _logger.LogWarning(
                "Login attempt for locked account {UserId}. Locked until {LockedUntil}",
                user.Id,
                user.LockedOutUntil);

            throw new UnauthorizedException(
                $"Account is locked due to multiple failed login attempts. Please try again in {minutesRemaining} minute(s).");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            null!,
            user.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= app.MaxFailedLoginAttempts)
            {
                user.LockedOutUntil = DateTime.UtcNow.AddMinutes(app.LockoutDurationMinutes);

                _logger.LogWarning(
                    "User {UserId} locked out after {Attempts} failed attempts. Locked until {LockedUntil}",
                    user.Id,
                    user.FailedLoginAttempts,
                    user.LockedOutUntil);
            }
            else
            {
                _logger.LogWarning(
                    "Failed login attempt {Attempts}/{Max} for user {UserId}",
                    user.FailedLoginAttempts,
                    app.MaxFailedLoginAttempts,
                    user.Id);
            }

            await _context.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid email or password");
        }

        if (app.RequireEmailVerification && !user.EmailVerified)
        {
            _logger.LogWarning(
                "Login attempt with unverified email for user {UserId}",
                user.Id);
            throw new UnauthorizedException(
                "Please verify your email address before logging in. Check your inbox for the verification email.");
        }

        user.FailedLoginAttempts = 0;
        user.LockedOutUntil = null;
        user.LastLoginAtUtc = DateTime.UtcNow;

        var tokens = await _jwtService.GenerateUserTokenPairAsync(
            user.Id,
            user.Email,
            app.Id,
            app.JwtSecretEncrypted,
            app.AccessTokenExpirationMinutes,
            app.RefreshTokenExpirationDays);

        var refreshToken = new Entities.UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokens.RefreshToken,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.UserRefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} logged in successfully to application {AppId}",
            user.Id,
            applicationId);

        return new UserLoginResponse(
            User: MapToDto(user),
            Tokens: tokens
        );
    }

    private static UserDto MapToDto(Entities.User user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            EmailVerified: user.EmailVerified,
            CreatedAtUtc: user.CreatedAtUtc
        );
}

public static class UserLogin
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/login", async (
                Guid appId,
                UserLoginRequest request,
                UserLoginHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserLoginValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserLoginResponse>.Ok(response));
            })
            .WithName("UserLogin")
            .WithTags("User Auth")
            .AllowAnonymous();
    }
}
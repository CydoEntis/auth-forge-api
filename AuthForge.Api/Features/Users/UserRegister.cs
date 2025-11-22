using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthForge.Api.Features.Users;

public sealed record UserRegisterRequest(
    string Email,
    string Password,
    string? FirstName,
    string? LastName
);

public sealed record UserRegisterResponse(
    UserDto? User,
    TokenPair? Tokens,
    string Message
);

public sealed record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified,
    DateTime CreatedAtUtc
);

public sealed class UserRegisterValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));
    }
}

public sealed class UserRegisterHandler
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailServiceFactory _emailServiceFactory;
    private readonly PasswordHasher<Entities.User> _passwordHasher;
    private readonly ILogger<UserRegisterHandler> _logger;

    public UserRegisterHandler(
        AppDbContext context,
        IJwtService jwtService,
        IEmailTemplateService emailTemplateService,
        IEmailServiceFactory emailServiceFactory,
        PasswordHasher<Entities.User> passwordHasher,
        ILogger<UserRegisterHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _emailTemplateService = emailTemplateService;
        _emailServiceFactory = emailServiceFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<UserRegisterResponse> HandleAsync(
        Guid applicationId,
        UserRegisterRequest request,
        CancellationToken ct)
    {
        var app = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (app == null || !app.IsActive)
        {
            _logger.LogWarning("Registration attempt for invalid application: {AppId}", applicationId);
            throw new BadRequestException("Application not found or inactive");
        }

        var existingUser = await _context.Users
            .AnyAsync(u => u.ApplicationId == applicationId && u.Email == request.Email, ct);

        if (existingUser)
        {
            _logger.LogWarning(
                "Registration attempt with existing email {Email} for app {AppId}",
                request.Email,
                applicationId);
            throw new ConflictException("An account with this email already exists");
        }

        var user = new Entities.User
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailVerified = false,
            FailedLoginAttempts = 0,
            CreatedAtUtc = DateTime.UtcNow
        };

        var verificationToken = GenerateVerificationToken();
        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} registered for application {AppId}",
            user.Id,
            applicationId);

        try
        {
            await SendVerificationEmailAsync(user, app, verificationToken, ct);
            _logger.LogInformation("Verification email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
        }

        TokenPair? tokens = null;
        string message;

        if (!app.RequireEmailVerification)
        {
            tokens = await _jwtService.GenerateUserTokenPairAsync(
                user.Id,
                user.Email,
                app.Id,
                app.JwtSecretEncrypted,
                app.AccessTokenExpirationMinutes,
                app.RefreshTokenExpirationDays);

            // Store refresh token
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

            message = "Registration successful";
        }
        else
        {
            message = "Registration successful. Please check your email to verify your account.";
        }

        return new UserRegisterResponse(
            User: MapToDto(user),
            Tokens: tokens,
            Message: message
        );
    }

    private static string GenerateVerificationToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private async Task SendVerificationEmailAsync(
        Entities.User user,
        Entities.Application app,
        string verificationToken,
        CancellationToken ct)
    {
        var verificationUrl = $"{app.EmailVerificationCallbackUrl}?token={verificationToken}";

        var emailMessage = await _emailTemplateService.CreateEmailVerificationEmailAsync(
            toEmail: user.Email,
            toName: user.FirstName ?? "User",
            verificationUrl: verificationUrl,
            appName: app.Name
        );

        string fromAddress;
        string? fromName;

        if (app.UseGlobalEmailSettings)
        {
            fromAddress = await _emailServiceFactory.GetFromAddressAsync(ct);
            fromName = await _emailServiceFactory.GetFromNameAsync(ct);
        }
        else
        {
            fromAddress = app.FromEmail ?? throw new InvalidOperationException("Application email not configured");
            fromName = app.FromName;
        }

        var finalMessage = emailMessage with
        {
            From = fromAddress,
            FromName = fromName ?? app.Name
        };

        var emailService = await _emailServiceFactory.CreateAsync(ct);
        var result = await emailService.SendAsync(finalMessage, ct);

        if (!result.Success)
        {
            _logger.LogError("Failed to send verification email: {Error}", result.ErrorMessage);
            throw new InvalidOperationException("Failed to send verification email");
        }
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

public static class UserRegister
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/register", async (
                Guid appId,
                UserRegisterRequest request,
                UserRegisterHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserRegisterValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserRegisterResponse>.Ok(response));
            })
            .WithName("UserRegister")
            .WithTags("User Auth")
            .AllowAnonymous();
    }
}
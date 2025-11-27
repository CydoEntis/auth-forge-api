using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Entities;
using AuthForge.Api.Features.Applications.Shared.Validators;
using AuthForge.Api.Features.Shared.Models;
using AuthForge.Api.Features.Shared.Validators;
using FluentValidation;

namespace AuthForge.Api.Features.Applications;

public sealed record CreateApplicationRequest(
    string Name,
    string? Description,
    List<string>? AllowedOrigins,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl,
    string? MagicLinkCallbackUrl,
    bool RequireEmailVerification = true,
    bool UseGlobalEmailSettings = true,
    EmailProviderConfig? EmailProviderConfig = null,
    OAuthSettings? OAuthSettings = null
);

public sealed record OAuthSettings(
    bool GoogleEnabled = false,
    string? GoogleClientId = null,
    string? GoogleClientSecret = null,
    bool GithubEnabled = false,
    string? GithubClientId = null,
    string? GithubClientSecret = null
);

public sealed record CreateApplicationResponse(
    Guid Id,
    string Name,
    string Slug,
    string ClientId,
    string ClientSecret,
    DateTime CreatedAtUtc
);

public class CreateApplicationRequestValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Application name is required")
            .MinimumLength(3).WithMessage("Application name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Application name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        When(x => x.AllowedOrigins != null && x.AllowedOrigins.Any(), () =>
        {
            RuleForEach(x => x.AllowedOrigins)
                .MustBeValidUrl();

            RuleFor(x => x.AllowedOrigins)
                .Must(x => x!.Count <= 10)
                .WithMessage("Maximum 10 allowed origins");
        });

        When(x => !string.IsNullOrEmpty(x.PasswordResetCallbackUrl), () =>
        {
            RuleFor(x => x.PasswordResetCallbackUrl)
                .MustBeValidUrl()
                .WithMessage("Password reset callback URL must be a valid URL");
        });

        When(x => !string.IsNullOrEmpty(x.EmailVerificationCallbackUrl), () =>
        {
            RuleFor(x => x.EmailVerificationCallbackUrl)
                .MustBeValidUrl()
                .WithMessage("Email verification callback URL must be a valid URL");
        });

        When(x => !string.IsNullOrEmpty(x.MagicLinkCallbackUrl), () =>
        {
            RuleFor(x => x.MagicLinkCallbackUrl)
                .MustBeValidUrl()
                .WithMessage("Magic link callback URL must be a valid URL");
        });

        When(x => !x.UseGlobalEmailSettings, () =>
        {
            RuleFor(x => x.EmailProviderConfig)
                .NotNull()
                .WithMessage("Email provider configuration is required when not using global settings")
                .SetValidator(new EmailProviderConfigValidator()!);
        });

        When(x => x.OAuthSettings != null && x.OAuthSettings.GoogleEnabled, () =>
        {
            RuleFor(x => x.OAuthSettings!.GoogleClientId)
                .NotEmpty()
                .WithMessage("Google Client ID is required");

            RuleFor(x => x.OAuthSettings!.GoogleClientSecret)
                .NotEmpty()
                .WithMessage("Google Client Secret is required");
        });

        When(x => x.OAuthSettings != null && x.OAuthSettings.GithubEnabled, () =>
        {
            RuleFor(x => x.OAuthSettings!.GithubClientId)
                .NotEmpty()
                .WithMessage("GitHub Client ID is required");

            RuleFor(x => x.OAuthSettings!.GithubClientSecret)
                .NotEmpty()
                .WithMessage("GitHub Client Secret is required");
        });
    }
}

public class CreateApplicationHandler
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<CreateApplicationHandler> _logger;

    public CreateApplicationHandler(
        AppDbContext context,
        IEncryptionService encryptionService,
        IJwtService jwtService,
        ILogger<CreateApplicationHandler> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<CreateApplicationResponse> HandleAsync(
        CreateApplicationRequest request,
        CancellationToken ct)
    {
        var slug = GenerateAppSlug(request.Name);
        var clientId = GenerateClientId();
        var clientSecret = _jwtService.GenerateUrlSafeToken(32);
        var jwtSecret = _jwtService.GenerateSecureToken(64);

        var application = new Application
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ClientId = clientId,
            ClientSecretEncrypted = _encryptionService.Encrypt(clientSecret),
            JwtSecretEncrypted = _encryptionService.Encrypt(jwtSecret),
            AllowedOrigins = request.AllowedOrigins ?? new List<string>(),
            PasswordResetCallbackUrl = request.PasswordResetCallbackUrl,
            EmailVerificationCallbackUrl = request.EmailVerificationCallbackUrl,
            MagicLinkCallbackUrl = request.MagicLinkCallbackUrl,
            IsActive = true,
            MaxFailedLoginAttempts = 5,
            LockoutDurationMinutes = 15,
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7,
            CreatedAtUtc = DateTime.UtcNow,
            RequireEmailVerification = request.RequireEmailVerification
        };

        if (!request.UseGlobalEmailSettings && request.EmailProviderConfig != null)
        {
            application.EmailSettings = new Entities.ApplicationEmailSettings
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                UseGlobalSettings = false,
                Provider = request.EmailProviderConfig.EmailProvider.ToString(),
                FromEmail = request.EmailProviderConfig.FromEmail,
                FromName = request.EmailProviderConfig.FromName,
                SmtpHost = request.EmailProviderConfig.SmtpHost,
                SmtpPort = request.EmailProviderConfig.SmtpPort,
                SmtpUsername = request.EmailProviderConfig.SmtpUsername,
                SmtpPasswordEncrypted = !string.IsNullOrEmpty(request.EmailProviderConfig.SmtpPassword)
                    ? _encryptionService.Encrypt(request.EmailProviderConfig.SmtpPassword)
                    : null,
                SmtpUseSsl = request.EmailProviderConfig.UseSsl,
                ResendApiKeyEncrypted = !string.IsNullOrEmpty(request.EmailProviderConfig.ResendApiKey)
                    ? _encryptionService.Encrypt(request.EmailProviderConfig.ResendApiKey)
                    : null,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        else
        {
            application.EmailSettings = new Entities.ApplicationEmailSettings
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                UseGlobalSettings = true,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        if (request.OAuthSettings != null &&
            (request.OAuthSettings.GoogleEnabled || request.OAuthSettings.GithubEnabled))
        {
            application.OAuthSettings = new Entities.ApplicationOAuthSettings
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                GoogleEnabled = request.OAuthSettings.GoogleEnabled,
                GoogleClientId = request.OAuthSettings.GoogleClientId,
                GoogleClientSecretEncrypted = !string.IsNullOrEmpty(request.OAuthSettings.GoogleClientSecret)
                    ? _encryptionService.Encrypt(request.OAuthSettings.GoogleClientSecret)
                    : null,
                GithubEnabled = request.OAuthSettings.GithubEnabled,
                GithubClientId = request.OAuthSettings.GithubClientId,
                GithubClientSecretEncrypted = !string.IsNullOrEmpty(request.OAuthSettings.GithubClientSecret)
                    ? _encryptionService.Encrypt(request.OAuthSettings.GithubClientSecret)
                    : null,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        _context.Applications.Add(application);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Application created: {Name} with ID {Id} and ClientId {ClientId}",
            application.Name, application.Id, application.ClientId);

        return new CreateApplicationResponse(
            application.Id,
            application.Name,
            application.Slug,
            application.ClientId,
            clientSecret,
            application.CreatedAtUtc
        );
    }

    private static string GenerateAppSlug(string applicationName)
    {
        var slug = applicationName.ToLowerInvariant().Replace(" ", "-").Replace("_", "-");
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{slug}-{suffix}";
    }

    private static string GenerateClientId()
    {
        return $"af_{Guid.NewGuid():N}";
    }
}

public static class CreateApplication
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/applications", async (
                CreateApplicationRequest request,
                CreateApplicationHandler handler,
                CancellationToken ct) =>
            {
                var validator = new CreateApplicationRequestValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<CreateApplicationResponse>.Ok(response));
            })
            .WithName("CreateApplication")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}
using System.Security.Cryptography;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Entities;
using AuthForge.Api.Features.Applications.Shared.Validators;
using FluentValidation;

namespace AuthForge.Api.Features.Applications;

public sealed record CreateApplicationRequest(
    string Name,
    string? Description,
    List<string>? AllowedOrigins,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl,
    string? MagicLinkCallbackUrl,
    bool RequireEmailVerification = true
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
    }
}

public class CreateApplicationHandler
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<CreateApplicationHandler> _logger;

    public CreateApplicationHandler(
        AppDbContext context,
        IEncryptionService encryptionService,
        ILogger<CreateApplicationHandler> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<CreateApplicationResponse> HandleAsync(
        CreateApplicationRequest request,
        CancellationToken ct)
    {
        var slug = GenerateAppSlug(request.Name);
        var clientId = GenerateClientId();
        var clientSecret = GeneratedClientSecret();
        var jwtSecret = GeneratedJwtSecret();

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
            UseGlobalEmailSettings = true,
            CreatedAtUtc = DateTime.UtcNow,
            RequireEmailVerification = request.RequireEmailVerification
        };

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

    private static string GeneratedClientSecret()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string GeneratedJwtSecret()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
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
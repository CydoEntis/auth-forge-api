using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Applications.Shared.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record UpdateApplicationEmailProviderRequest(
    bool UseGlobalEmailSettings,
    string? EmailProvider,
    string? FromEmail,
    string? FromName,
    string? EmailApiKey,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl
);

public sealed record UpdateApplicationEmailProviderResponse(
    Guid Id,
    string Name,
    bool UseGlobalEmailSettings,
    string? FromEmail,
    DateTime UpdatedAtUtc
);

public class UpdateApplicationEmailValidator : AbstractValidator<UpdateApplicationEmailProviderRequest>
{
    public UpdateApplicationEmailValidator()
    {
        When(x => !x.UseGlobalEmailSettings, () =>
        {
            RuleFor(x => x.EmailProvider)
                .NotEmpty().WithMessage("Email provider is required when not using global settings");

            RuleFor(x => x.FromEmail)
                .NotEmpty().WithMessage("From email is required when not using global settings")
                .EmailAddress().WithMessage("Must be a valid email address");

            When(x => !string.IsNullOrEmpty(x.PasswordResetCallbackUrl), () =>
            {
                RuleFor(x => x.PasswordResetCallbackUrl)
                    .MustBeValidUrl();
            });

            When(x => !string.IsNullOrEmpty(x.EmailVerificationCallbackUrl), () =>
            {
                RuleFor(x => x.EmailVerificationCallbackUrl)
                    .MustBeValidUrl();
            });
        });
    }
}

public class UpdateApplicationEmailProviderHandler
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UpdateApplicationEmailProviderHandler> _logger;

    public UpdateApplicationEmailProviderHandler(
        AppDbContext context,
        IEncryptionService encryptionService,
        ILogger<UpdateApplicationEmailProviderHandler> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<UpdateApplicationEmailProviderResponse> HandleAsync(
        Guid id,
        UpdateApplicationEmailProviderRequest providerRequest,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application with ID {id} not found");
        }

        application.UseGlobalEmailSettings = providerRequest.UseGlobalEmailSettings;

        if (providerRequest.UseGlobalEmailSettings)
        {
            application.EmailProvider = null;
            application.FromEmail = null;
            application.FromName = null;
            application.EmailApiKeyEncrypted = null;
        }
        else
        {
            application.EmailProvider = providerRequest.EmailProvider;
            application.FromEmail = providerRequest.FromEmail;
            application.FromName = providerRequest.FromName;

            if (!string.IsNullOrEmpty(providerRequest.EmailApiKey))
            {
                application.EmailApiKeyEncrypted = _encryptionService.Encrypt(providerRequest.EmailApiKey);
            }
        }

        application.PasswordResetCallbackUrl = providerRequest.PasswordResetCallbackUrl;
        application.EmailVerificationCallbackUrl = providerRequest.EmailVerificationCallbackUrl;
        application.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated email settings for application: {Name} ({Id})", application.Name,
            application.Id);

        return new UpdateApplicationEmailProviderResponse(
            application.Id,
            application.Name,
            application.UseGlobalEmailSettings,
            application.FromEmail,
            application.UpdatedAtUtc.Value
        );
    }
}

public static class UpdateApplicationEmailProvider
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/applications/{{id:guid}}/email", async (
                Guid id,
                UpdateApplicationEmailProviderRequest providerRequest,
                UpdateApplicationEmailProviderHandler providerHandler,
                CancellationToken ct) =>
            {
                var validator = new UpdateApplicationEmailValidator();
                var validationResult = await validator.ValidateAsync(providerRequest, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await providerHandler.HandleAsync(id, providerRequest, ct);
                return Results.Ok(ApiResponse<UpdateApplicationEmailProviderResponse>.Ok(response));
            })
            .WithName("UpdateApplicationEmail")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}
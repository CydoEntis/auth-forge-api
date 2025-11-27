using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Applications.Shared.Validators;
using AuthForge.Api.Features.Shared.Enums;
using AuthForge.Api.Features.Shared.Models;
using AuthForge.Api.Features.Shared.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record UpdateApplicationEmailProviderRequest(
    bool UseGlobalEmailSettings,
    EmailProviderConfig? EmailProviderConfig,
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
            RuleFor(x => x.EmailProviderConfig)
                .NotNull()
                .WithMessage("Email provider configuration is required when not using global settings")
                .SetValidator(new EmailProviderConfigValidator()!);
        });

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
        UpdateApplicationEmailProviderRequest request,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .Include(a => a.EmailSettings)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application with ID {id} not found");
        }

        if (application.EmailSettings == null)
        {
            application.EmailSettings = new Entities.ApplicationEmailSettings
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.ApplicationEmailSettings.Add(application.EmailSettings);
        }

        application.EmailSettings.UseGlobalSettings = request.UseGlobalEmailSettings;

        if (request.UseGlobalEmailSettings)
        {
            application.EmailSettings.Provider = null;
            application.EmailSettings.FromEmail = null;
            application.EmailSettings.FromName = null;
            application.EmailSettings.SmtpHost = null;
            application.EmailSettings.SmtpPort = null;
            application.EmailSettings.SmtpUsername = null;
            application.EmailSettings.SmtpPasswordEncrypted = null;
            application.EmailSettings.SmtpUseSsl = true;
            application.EmailSettings.ResendApiKeyEncrypted = null;
        }
        else
        {
            var config = request.EmailProviderConfig!;

            application.EmailSettings.Provider = config.EmailProvider.ToString();
            application.EmailSettings.FromEmail = config.FromEmail;
            application.EmailSettings.FromName = config.FromName;

            if (config.EmailProvider == EmailProvider.Smtp)
            {
                application.EmailSettings.SmtpHost = config.SmtpHost;
                application.EmailSettings.SmtpPort = config.SmtpPort;
                application.EmailSettings.SmtpUsername = config.SmtpUsername;
                application.EmailSettings.SmtpPasswordEncrypted = !string.IsNullOrEmpty(config.SmtpPassword)
                    ? _encryptionService.Encrypt(config.SmtpPassword)
                    : null;
                application.EmailSettings.SmtpUseSsl = config.UseSsl;
                application.EmailSettings.ResendApiKeyEncrypted = null;
            }
            else if (config.EmailProvider == EmailProvider.Resend)
            {
                application.EmailSettings.ResendApiKeyEncrypted = !string.IsNullOrEmpty(config.ResendApiKey)
                    ? _encryptionService.Encrypt(config.ResendApiKey)
                    : null;
                application.EmailSettings.SmtpHost = null;
                application.EmailSettings.SmtpPort = null;
                application.EmailSettings.SmtpUsername = null;
                application.EmailSettings.SmtpPasswordEncrypted = null;
                application.EmailSettings.SmtpUseSsl = true;
            }
        }

        application.EmailSettings.UpdatedAtUtc = DateTime.UtcNow;
        application.PasswordResetCallbackUrl = request.PasswordResetCallbackUrl;
        application.EmailVerificationCallbackUrl = request.EmailVerificationCallbackUrl;
        application.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated email settings for application: {Name} ({Id})",
            application.Name, application.Id);

        return new UpdateApplicationEmailProviderResponse(
            application.Id,
            application.Name,
            application.EmailSettings.UseGlobalSettings,
            application.EmailSettings.FromEmail,
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
                UpdateApplicationEmailProviderRequest request,
                [FromServices] UpdateApplicationEmailProviderHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UpdateApplicationEmailValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(id, request, ct);
                return Results.Ok(ApiResponse<UpdateApplicationEmailProviderResponse>.Ok(response));
            })
            .WithName("UpdateApplicationEmail")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}
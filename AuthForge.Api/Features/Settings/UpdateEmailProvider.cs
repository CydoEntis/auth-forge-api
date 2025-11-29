using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Enums;
using AuthForge.Api.Features.Shared.Models;
using AuthForge.Api.Features.Shared.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Settings;

public sealed record UpdateEmailProviderRequest(
    EmailProviderConfig EmailProviderConfig
);

public sealed record UpdateEmailProviderResponse(string Message);

public class UpdateEmailProviderHandler
{
    private readonly ConfigDbContext _configDb;
    private readonly IEncryptionService _encryptionService;

    public UpdateEmailProviderHandler(ConfigDbContext configDb, IEncryptionService encryptionService)
    {
        _configDb = configDb;
        _encryptionService = encryptionService;
    }

    public async Task<UpdateEmailProviderResponse> HandleAsync(
        UpdateEmailProviderRequest request,
        CancellationToken ct)
    {
        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);

        if (config == null)
            throw new NotFoundException("Configuration not found");

        config.EmailProvider = request.EmailProviderConfig.EmailProvider.ToString();
        config.EmailFromAddress = request.EmailProviderConfig.FromEmail;
        config.EmailFromName = request.EmailProviderConfig.FromName;

        // SMTP
        if (request.EmailProviderConfig.EmailProvider == EmailProvider.Smtp)
        {
            config.SmtpHost = request.EmailProviderConfig.SmtpHost;
            config.SmtpPort = request.EmailProviderConfig.SmtpPort;
            config.SmtpUsername = request.EmailProviderConfig.SmtpUsername;
            config.SmtpPasswordEncrypted = !string.IsNullOrEmpty(request.EmailProviderConfig.SmtpPassword)
                ? _encryptionService.Encrypt(request.EmailProviderConfig.SmtpPassword)
                : config.SmtpPasswordEncrypted;
            config.SmtpUseSsl = request.EmailProviderConfig.UseSsl;

            config.ResendApiKeyEncrypted = null;
        }
        // Resend
        else
        {
            config.ResendApiKeyEncrypted = !string.IsNullOrEmpty(request.EmailProviderConfig.ResendApiKey)
                ? _encryptionService.Encrypt(request.EmailProviderConfig.ResendApiKey)
                : config.ResendApiKeyEncrypted;

            config.SmtpHost = null;
            config.SmtpPort = null;
            config.SmtpUsername = null;
            config.SmtpPasswordEncrypted = null;
        }

        config.UpdatedAtUtc = DateTime.UtcNow;

        await _configDb.SaveChangesAsync(ct);

        return new UpdateEmailProviderResponse("Email provider updated successfully");
    }
}

public static class UpdateEmailProvider
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/email-provider", async (
                UpdateEmailProviderRequest request,
                UpdateEmailProviderHandler handler,
                CancellationToken ct) =>
            {
                var validator = new EmailProviderConfigValidator();
                var validationResult = await validator.ValidateAsync(request.EmailProviderConfig, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<UpdateEmailProviderResponse>.Ok(response));
            })
            .WithName("UpdateEmailProvider")
            .WithTags("Settings")
            .RequireAuthorization();
    }
}
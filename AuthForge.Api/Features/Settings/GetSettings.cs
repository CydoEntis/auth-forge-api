using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Enums;
using AuthForge.Api.Features.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Settings;

public sealed record SettingsResponse(
    string Email,
    string AuthForgeDomain,
    EmailProviderConfig EmailProvider
);

public class GetSettingsHandler
{
    private readonly AppDbContext _appDb;
    private readonly ConfigDbContext _configDb;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetSettingsHandler> _logger;

    public GetSettingsHandler(
        AppDbContext appDb,
        ConfigDbContext configDb,
        ICurrentUserService currentUser,
        ILogger<GetSettingsHandler> logger)
    {
        _appDb = appDb;
        _configDb = configDb;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<SettingsResponse> HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var adminId))
            throw new UnauthorizedException("Invalid user ID");

        var admin = await _appDb.Admins
            .FirstOrDefaultAsync(a => a.Id == adminId, ct);

        if (admin == null)
            throw new NotFoundException("Account not found");

        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);
        if (config == null)
            throw new NotFoundException("Configuration not found");

        _logger.LogInformation("Retrieved settings");

        var emailProvider = Enum.TryParse<EmailProvider>(config.EmailProvider, out var provider)
            ? provider
            : EmailProvider.Smtp;

        return new SettingsResponse(
            Email: admin.Email,
            AuthForgeDomain: config.AuthForgeDomain ?? "http://localhost:3000",
            EmailProvider: new EmailProviderConfig
            {
                EmailProvider = emailProvider,
                FromEmail = config.EmailFromAddress ?? "noreply@example.com",
                FromName = config.EmailFromName,
                SmtpHost = config.SmtpHost,
                SmtpPort = config.SmtpPort,
                SmtpUsername = config.SmtpUsername,
                SmtpPassword = null,
                UseSsl = config.SmtpUseSsl,
                ResendApiKey = null
            }
        );
    }
}

public static class GetSettings
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}", async (
                [FromServices] GetSettingsHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<SettingsResponse>.Ok(response));
            })
            .WithName("GetSettings")
            .WithTags("Settings")
            .RequireAuthorization();
    }
}
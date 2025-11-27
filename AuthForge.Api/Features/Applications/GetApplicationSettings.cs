using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record GetApplicationSettingsResponse(
    Guid Id,
    string Name,
    string? Description,
    List<string> AllowedOrigins,
    bool IsActive,
    ApplicationSecuritySettings Security,
    ApplicationOAuthSettings OAuth,
    ApplicationEmailSettings Email
);

public sealed record ApplicationSecuritySettings(
    int MaxFailedLoginAttempts,
    int LockoutDurationMinutes,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays
);

public sealed record ApplicationOAuthSettings(
    bool GoogleEnabled,
    string? GoogleClientId,
    bool GithubEnabled,
    string? GithubClientId
);

public sealed record ApplicationEmailSettings(
    bool UseGlobalEmailSettings,
    string? EmailProvider,
    string? FromEmail,
    string? FromName,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl,
    string? MagicLinkCallbackUrl
);

public class GetApplicationSettingsHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetApplicationSettingsHandler> _logger;

    public GetApplicationSettingsHandler(AppDbContext context, ILogger<GetApplicationSettingsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetApplicationSettingsResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications
            .Include(a => a.EmailSettings)
            .Include(a => a.OAuthSettings)
            .Where(a => a.Id == id)
            .Select(a => new GetApplicationSettingsResponse(
                a.Id,
                a.Name,
                a.Description,
                a.AllowedOrigins,
                a.IsActive,
                new ApplicationSecuritySettings(
                    a.MaxFailedLoginAttempts,
                    a.LockoutDurationMinutes,
                    a.AccessTokenExpirationMinutes,
                    a.RefreshTokenExpirationDays
                ),
                new ApplicationOAuthSettings(
                    a.OAuthSettings != null && a.OAuthSettings.GoogleEnabled,
                    a.OAuthSettings != null ? a.OAuthSettings.GoogleClientId : null,
                    a.OAuthSettings != null && a.OAuthSettings.GithubEnabled,
                    a.OAuthSettings != null ? a.OAuthSettings.GithubClientId : null
                ),
                new ApplicationEmailSettings(
                    a.EmailSettings != null && a.EmailSettings.UseGlobalSettings,
                    a.EmailSettings != null ? a.EmailSettings.Provider : null,
                    a.EmailSettings != null ? a.EmailSettings.FromEmail : null,
                    a.EmailSettings != null ? a.EmailSettings.FromName : null,
                    a.PasswordResetCallbackUrl,
                    a.EmailVerificationCallbackUrl,
                    a.MagicLinkCallbackUrl
                )
            ))
            .FirstOrDefaultAsync(ct);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {Id}", id);
            throw new NotFoundException($"Application with ID {id} not found");
        }

        _logger.LogInformation("Retrieved settings for application: {Name} ({Id})", application.Name, application.Id);

        return application;
    }
}

public static class GetApplicationSettings
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/applications/{{id:guid}}/settings", async (
                Guid id,
                [FromServices] GetApplicationSettingsHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(id, ct);
                return Results.Ok(ApiResponse<GetApplicationSettingsResponse>.Ok(response));
            })
            .WithName("GetApplicationSettings")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}
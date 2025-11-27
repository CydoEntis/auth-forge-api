using System.Security.Cryptography;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record GoogleAuthorizeResponse(string AuthorizationUrl);

public sealed class GoogleAuthorizeHandler
{
    private readonly AppDbContext _context;
    private readonly IOAuthService _oauthService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<GoogleAuthorizeHandler> _logger;

    public GoogleAuthorizeHandler(
        AppDbContext context,
        IOAuthService oauthService,
        IEncryptionService encryptionService,
        ILogger<GoogleAuthorizeHandler> logger)
    {
        _context = context;
        _oauthService = oauthService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<GoogleAuthorizeResponse> HandleAsync(
        Guid applicationId,
        string redirectUri,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .Include(a => a.OAuthSettings)
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (application == null || !application.IsActive)
        {
            throw new NotFoundException($"Application {applicationId} not found or inactive");
        }

        if (application.OAuthSettings?.GoogleEnabled != true ||
            string.IsNullOrEmpty(application.OAuthSettings.GoogleClientId))
        {
            throw new BadRequestException("Google OAuth is not enabled for this application");
        }

        var stateBytes = RandomNumberGenerator.GetBytes(32);
        var state = Convert.ToBase64String(stateBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var authUrl = _oauthService.GetAuthorizationUrl(
            provider: "google",
            clientId: application.OAuthSettings.GoogleClientId,
            redirectUri: redirectUri,
            state: $"{state}:{applicationId}"
        );

        _logger.LogInformation(
            "Generated Google OAuth authorization URL for application {AppId}",
            applicationId);

        return new GoogleAuthorizeResponse(authUrl);
    }
}

public static class GoogleAuthorize
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/apps/{{appId:guid}}/auth/google/authorize", async (
                Guid appId,
                string redirectUri,
                GoogleAuthorizeHandler handler,
                CancellationToken ct) =>
            {
                if (string.IsNullOrEmpty(redirectUri))
                {
                    return Results.BadRequest(ApiResponse.Fail(
                        ErrorCodes.ValidationFailed,
                        "redirectUri query parameter is required"));
                }

                var response = await handler.HandleAsync(appId, redirectUri, ct);
                return Results.Ok(ApiResponse<GoogleAuthorizeResponse>.Ok(response));
            })
            .WithName("GoogleAuthorize")
            .WithTags("OAuth")
            .AllowAnonymous();
    }
}
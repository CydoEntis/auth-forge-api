using System.Security.Cryptography;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record GithubAuthorizeResponse(string AuthorizationUrl);

public sealed class GithubAuthorizeHandler
{
    private readonly AppDbContext _context;
    private readonly IOAuthService _oauthService;
    private readonly ILogger<GithubAuthorizeHandler> _logger;

    public GithubAuthorizeHandler(
        AppDbContext context,
        IOAuthService oauthService,
        ILogger<GithubAuthorizeHandler> logger)
    {
        _context = context;
        _oauthService = oauthService;
        _logger = logger;
    }

    public async Task<GithubAuthorizeResponse> HandleAsync(
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

        if (application.OAuthSettings?.GithubEnabled != true ||
            string.IsNullOrEmpty(application.OAuthSettings.GithubClientId))
        {
            throw new BadRequestException("GitHub OAuth is not enabled for this application");
        }

        var stateBytes = RandomNumberGenerator.GetBytes(32);
        var state = Convert.ToBase64String(stateBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var authUrl = _oauthService.GetAuthorizationUrl(
            provider: "github",
            clientId: application.OAuthSettings.GithubClientId,
            redirectUri: redirectUri,
            state: $"{state}:{applicationId}"
        );

        _logger.LogInformation(
            "Generated GitHub OAuth authorization URL for application {AppId}",
            applicationId);

        return new GithubAuthorizeResponse(authUrl);
    }
}

public static class GithubAuthorize
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/apps/{{appId:guid}}/auth/github/authorize", async (
                Guid appId,
                string redirectUri,
                GithubAuthorizeHandler handler,
                CancellationToken ct) =>
            {
                if (string.IsNullOrEmpty(redirectUri))
                {
                    return Results.BadRequest(ApiResponse.Fail(
                        ErrorCodes.ValidationFailed,
                        "redirectUri query parameter is required"));
                }

                var response = await handler.HandleAsync(appId, redirectUri, ct);
                return Results.Ok(ApiResponse<GithubAuthorizeResponse>.Ok(response));
            })
            .WithName("GithubAuthorize")
            .WithTags("OAuth")
            .AllowAnonymous();
    }
}
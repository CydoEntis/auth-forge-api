using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record GithubCallbackResponse(
    TokenPair Tokens,
    bool IsNewUser
);

public sealed class GithubCallbackHandler
{
    private readonly AppDbContext _context;
    private readonly IOAuthService _oauthService;
    private readonly IJwtService _jwtService;
    private readonly IEncryptionService _encryptionService;
    private readonly PasswordHasher<Entities.User> _passwordHasher;
    private readonly ILogger<GithubCallbackHandler> _logger;

    public GithubCallbackHandler(
        AppDbContext context,
        IOAuthService oauthService,
        IJwtService jwtService,
        IEncryptionService encryptionService,
        PasswordHasher<Entities.User> passwordHasher,
        ILogger<GithubCallbackHandler> logger)
    {
        _context = context;
        _oauthService = oauthService;
        _jwtService = jwtService;
        _encryptionService = encryptionService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<GithubCallbackResponse> HandleAsync(
        Guid applicationId,
        string code,
        string state,
        string redirectUri,
        CancellationToken ct)
    {
        var stateParts = state.Split(':');
        if (stateParts.Length != 2 || !Guid.TryParse(stateParts[1], out var stateAppId) || stateAppId != applicationId)
        {
            throw new BadRequestException("Invalid state parameter");
        }

        var application = await _context.Applications
            .Include(a => a.OAuthSettings) // ✅ ADDED
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (application == null || !application.IsActive)
        {
            throw new NotFoundException($"Application {applicationId} not found or inactive");
        }

        if (application.OAuthSettings?.GithubEnabled != true ||
            string.IsNullOrEmpty(application.OAuthSettings.GithubClientId) ||
            string.IsNullOrEmpty(application.OAuthSettings.GithubClientSecretEncrypted))
        {
            throw new BadRequestException("GitHub OAuth is not enabled for this application");
        }

        var clientSecret =
            _encryptionService.Decrypt(application.OAuthSettings.GithubClientSecretEncrypted);

        var tokenResponse = await _oauthService.ExchangeCodeForTokenAsync(
            provider: "github",
            code: code,
            clientId: application.OAuthSettings.GithubClientId,
            clientSecret: clientSecret,
            redirectUri: redirectUri,
            ct: ct);

        var userInfo = await _oauthService.GetUserInfoAsync(
            provider: "github",
            accessToken: tokenResponse.AccessToken,
            ct: ct);

        var existingIdentity = await _context.UserOAuthIdentities
            .Include(o => o.User)
            .FirstOrDefaultAsync(o =>
                o.Provider == "github" &&
                o.ProviderUserId == userInfo.ProviderId &&
                o.User.ApplicationId == applicationId, ct);

        Entities.User user;
        bool isNewUser;

        if (existingIdentity != null)
        {
            user = existingIdentity.User;
            isNewUser = false;

            existingIdentity.ProviderEmail = userInfo.Email;
            existingIdentity.ProviderDisplayName = userInfo.Name;
            existingIdentity.ProviderAvatarUrl = userInfo.AvatarUrl;
            existingIdentity.UpdatedAtUtc = DateTime.UtcNow;

            if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(userInfo.FirstName))
                user.FirstName = userInfo.FirstName;
            if (string.IsNullOrEmpty(user.LastName) && !string.IsNullOrEmpty(userInfo.LastName))
                user.LastName = userInfo.LastName;
            if (!user.EmailVerified && userInfo.EmailVerified)
                user.EmailVerified = true;

            user.LastLoginAtUtc = DateTime.UtcNow;
            user.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.ApplicationId == applicationId &&
                    u.Email == userInfo.Email, ct);

            if (existingUser != null)
            {
                user = existingUser;
                isNewUser = false;

                var newIdentity = new Entities.UserOAuthIdentity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Provider = "github",
                    ProviderUserId = userInfo.ProviderId,
                    ProviderEmail = userInfo.Email,
                    ProviderDisplayName = userInfo.Name,
                    ProviderAvatarUrl = userInfo.AvatarUrl,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.UserOAuthIdentities.Add(newIdentity);

                if (!user.EmailVerified && userInfo.EmailVerified)
                    user.EmailVerified = true;

                user.LastLoginAtUtc = DateTime.UtcNow;
                user.UpdatedAtUtc = DateTime.UtcNow;

                _logger.LogInformation(
                    "Linked GitHub account to existing user {UserId} in application {AppId}",
                    user.Id, applicationId);
            }
            else
            {
                user = new Entities.User
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = applicationId,
                    Email = userInfo.Email,
                    PasswordHash = _passwordHasher.HashPassword(null!, Guid.NewGuid().ToString()),
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    EmailVerified = userInfo.EmailVerified,
                    FailedLoginAttempts = 0,
                    CreatedAtUtc = DateTime.UtcNow,
                    LastLoginAtUtc = DateTime.UtcNow
                };

                _context.Users.Add(user);

                var newIdentity = new Entities.UserOAuthIdentity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Provider = "github",
                    ProviderUserId = userInfo.ProviderId,
                    ProviderEmail = userInfo.Email,
                    ProviderDisplayName = userInfo.Name,
                    ProviderAvatarUrl = userInfo.AvatarUrl,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.UserOAuthIdentities.Add(newIdentity);
                isNewUser = true;

                _logger.LogInformation(
                    "Created new user {UserId} via GitHub OAuth in application {AppId}",
                    user.Id, applicationId);
            }
        }

        await _context.SaveChangesAsync(ct);

        var tokens = await _jwtService.GenerateUserTokenPairAsync(
            user.Id,
            user.Email,
            application.Id,
            application.JwtSecretEncrypted,
            application.AccessTokenExpirationMinutes,
            application.RefreshTokenExpirationDays);

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

        return new GithubCallbackResponse(tokens, isNewUser);
    }
}

public static class GithubCallback
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/apps/{{appId:guid}}/auth/github/callback", async (
                Guid appId,
                string code,
                string state,
                string redirectUri,
                GithubCallbackHandler handler,
                CancellationToken ct) =>
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Results.BadRequest(ApiResponse.Fail(
                        ErrorCodes.ValidationFailed,
                        "code query parameter is required"));
                }

                if (string.IsNullOrEmpty(state))
                {
                    return Results.BadRequest(ApiResponse.Fail(
                        ErrorCodes.ValidationFailed,
                        "state query parameter is required"));
                }

                if (string.IsNullOrEmpty(redirectUri))
                {
                    return Results.BadRequest(ApiResponse.Fail(
                        ErrorCodes.ValidationFailed,
                        "redirectUri query parameter is required"));
                }

                var response = await handler.HandleAsync(appId, code, state, redirectUri, ct);
                return Results.Ok(ApiResponse<GithubCallbackResponse>.Ok(response));
            })
            .WithName("GithubCallback")
            .WithTags("OAuth")
            .AllowAnonymous();
    }
}
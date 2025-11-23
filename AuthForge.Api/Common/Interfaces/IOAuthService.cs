namespace AuthForge.Api.Common.Interfaces;

public interface IOAuthService
{
    string GetAuthorizationUrl(
        string provider,
        string clientId,
        string redirectUri,
        string state,
        List<string>? scopes = null);

    Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(
        string provider,
        string code,
        string clientId,
        string clientSecret,
        string redirectUri,
        CancellationToken ct = default);

    Task<OAuthUserInfo> GetUserInfoAsync(
        string provider,
        string accessToken,
        CancellationToken ct = default);
}

public sealed record OAuthTokenResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    string TokenType
);

public sealed record OAuthUserInfo(
    string ProviderId,
    string Email,
    bool EmailVerified,
    string? Name,
    string? FirstName,
    string? LastName,
    string? AvatarUrl
);
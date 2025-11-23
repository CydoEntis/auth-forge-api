using System.Net.Http.Headers;
using System.Text.Json;
using AuthForge.Api.Common.Interfaces;

namespace AuthForge.Api.Common.Services;

public class OAuthService : IOAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IHttpClientFactory httpClientFactory,
        ILogger<OAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string GetAuthorizationUrl(
        string provider,
        string clientId,
        string redirectUri,
        string state,
        List<string>? scopes = null)
    {
        var scopeString = scopes != null && scopes.Any()
            ? string.Join(" ", scopes)
            : GetDefaultScopes(provider);

        return provider.ToLower() switch
        {
            "google" => $"https://accounts.google.com/o/oauth2/v2/auth?" +
                        $"client_id={Uri.EscapeDataString(clientId)}&" +
                        $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                        $"response_type=code&" +
                        $"scope={Uri.EscapeDataString(scopeString)}&" +
                        $"state={Uri.EscapeDataString(state)}&" +
                        $"access_type=offline&" +
                        $"prompt=consent",

            "github" => $"https://github.com/login/oauth/authorize?" +
                        $"client_id={Uri.EscapeDataString(clientId)}&" +
                        $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                        $"scope={Uri.EscapeDataString(scopeString)}&" +
                        $"state={Uri.EscapeDataString(state)}",

            _ => throw new NotSupportedException($"OAuth provider '{provider}' is not supported")
        };
    }

    public async Task<OAuthTokenResponse> ExchangeCodeForTokenAsync(
        string provider,
        string code,
        string clientId,
        string clientSecret,
        string redirectUri,
        CancellationToken ct = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        return provider.ToLower() switch
        {
            "google" => await ExchangeGoogleCodeAsync(httpClient, code, clientId, clientSecret, redirectUri, ct),
            "github" => await ExchangeGithubCodeAsync(httpClient, code, clientId, clientSecret, redirectUri, ct),
            _ => throw new NotSupportedException($"OAuth provider '{provider}' is not supported")
        };
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(
        string provider,
        string accessToken,
        CancellationToken ct = default)
    {
        var httpClient = _httpClientFactory.CreateClient();

        return provider.ToLower() switch
        {
            "google" => await GetGoogleUserInfoAsync(httpClient, accessToken, ct),
            "github" => await GetGithubUserInfoAsync(httpClient, accessToken, ct),
            _ => throw new NotSupportedException($"OAuth provider '{provider}' is not supported")
        };
    }

    private async Task<OAuthTokenResponse> ExchangeGoogleCodeAsync(
        HttpClient httpClient,
        string code,
        string clientId,
        string clientSecret,
        string redirectUri,
        CancellationToken ct)
    {
        var tokenRequest = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        };

        var response = await httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(tokenRequest),
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

        return new OAuthTokenResponse(
            AccessToken: tokenData.GetProperty("access_token").GetString()!,
            RefreshToken: tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
            ExpiresIn: tokenData.GetProperty("expires_in").GetInt32(),
            TokenType: tokenData.GetProperty("token_type").GetString()!
        );
    }

    private async Task<OAuthUserInfo> GetGoogleUserInfoAsync(
        HttpClient httpClient,
        string accessToken,
        CancellationToken ct)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var userData = JsonSerializer.Deserialize<JsonElement>(json);

        return new OAuthUserInfo(
            ProviderId: userData.GetProperty("id").GetString()!,
            Email: userData.GetProperty("email").GetString()!,
            EmailVerified: userData.TryGetProperty("verified_email", out var ve) && ve.GetBoolean(),
            Name: userData.TryGetProperty("name", out var n) ? n.GetString() : null,
            FirstName: userData.TryGetProperty("given_name", out var fn) ? fn.GetString() : null,
            LastName: userData.TryGetProperty("family_name", out var ln) ? ln.GetString() : null,
            AvatarUrl: userData.TryGetProperty("picture", out var p) ? p.GetString() : null
        );
    }

    private async Task<OAuthTokenResponse> ExchangeGithubCodeAsync(
        HttpClient httpClient,
        string code,
        string clientId,
        string clientSecret,
        string redirectUri,
        CancellationToken ct)
    {
        var tokenRequest = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri }
        };

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await httpClient.PostAsync(
            "https://github.com/login/oauth/access_token",
            new FormUrlEncodedContent(tokenRequest),
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

        return new OAuthTokenResponse(
            AccessToken: tokenData.GetProperty("access_token").GetString()!,
            RefreshToken: tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
            ExpiresIn: tokenData.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : 0,
            TokenType: tokenData.GetProperty("token_type").GetString()!
        );
    }

    private async Task<OAuthUserInfo> GetGithubUserInfoAsync(
        HttpClient httpClient,
        string accessToken,
        CancellationToken ct)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AuthForge", "1.0"));

        var userResponse = await httpClient.GetAsync("https://api.github.com/user", ct);
        userResponse.EnsureSuccessStatusCode();

        var userJson = await userResponse.Content.ReadAsStringAsync(ct);
        var userData = JsonSerializer.Deserialize<JsonElement>(userJson);

        var emailResponse = await httpClient.GetAsync("https://api.github.com/user/emails", ct);
        emailResponse.EnsureSuccessStatusCode();

        var emailJson = await emailResponse.Content.ReadAsStringAsync(ct);
        var emails = JsonSerializer.Deserialize<JsonElement[]>(emailJson);
        var primaryEmail = emails?.FirstOrDefault(e =>
            e.TryGetProperty("primary", out var p) && p.GetBoolean());

        string? email = null;

        if (primaryEmail.HasValue)
        {
            email = primaryEmail.Value.GetProperty("email").GetString();
        }
        else if (userData.TryGetProperty("email", out var emailProp) &&
                 emailProp.ValueKind != JsonValueKind.Null)
        {
            email = emailProp.GetString();
        }

        var emailVerified = primaryEmail.HasValue &&
                            primaryEmail.Value.TryGetProperty("verified", out var v) &&
                            v.GetBoolean();

        var name = userData.TryGetProperty("name", out var n) ? n.GetString() : null;

        return new OAuthUserInfo(
            ProviderId: userData.GetProperty("id").GetInt64().ToString(),
            Email: email ?? throw new InvalidOperationException("GitHub user has no email"),
            EmailVerified: emailVerified,
            Name: name,
            FirstName: name?.Split(' ').FirstOrDefault(),
            LastName: name?.Split(' ').Skip(1).FirstOrDefault(),
            AvatarUrl: userData.TryGetProperty("avatar_url", out var a) ? a.GetString() : null
        );
    }

    private static string GetDefaultScopes(string provider)
    {
        return provider.ToLower() switch
        {
            "google" => "openid email profile",
            "github" => "read:user user:email",
            _ => throw new NotSupportedException($"OAuth provider '{provider}' is not supported")
        };
    }
}
namespace AuthForge.Application.Auth.Commands.Refresh;

public sealed class RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    int ExpiresIn)
{
    public string TokenType => "Bearer";
}
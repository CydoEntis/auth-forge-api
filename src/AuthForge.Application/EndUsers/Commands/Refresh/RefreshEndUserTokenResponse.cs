namespace AuthForge.Application.EndUsers.Commands.Refresh;

public sealed record RefreshEndUserTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    int ExpiresIn,
    string TokenType = "Bearer");
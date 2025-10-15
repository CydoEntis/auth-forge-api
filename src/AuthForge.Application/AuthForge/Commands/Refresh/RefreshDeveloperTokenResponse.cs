namespace AuthForge.Application.AuthForge.Commands.Refresh;

public sealed record RefreshDeveloperTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    int ExpiresIn,
    string TokenType = "Bearer");
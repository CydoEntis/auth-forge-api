namespace AuthForge.Application.AuthForge.Commands.Login;

public sealed record LoginDeveloperResponse(
    string UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    int ExpiresIn,
    string TokenType = "Bearer");
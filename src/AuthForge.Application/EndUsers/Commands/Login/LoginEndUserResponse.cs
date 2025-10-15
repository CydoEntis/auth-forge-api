namespace AuthForge.Application.EndUsers.Commands.Login;

public sealed record LoginEndUserResponse(
    string UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    int ExpiresIn,
    string TokenType = "Bearer");
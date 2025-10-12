namespace AuthForge.Application.Auth.Commands.Login;

public sealed record LoginResponse(
    string UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    int ExpiresIn)
{
    public string TokenType => "Bearer";
}
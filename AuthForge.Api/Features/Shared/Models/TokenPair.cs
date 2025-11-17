namespace AuthForge.Api.Features.Shared.Models;

public sealed record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt)
{
    public int ExpiresInSeconds => (int)(AccessTokenExpiresAt - DateTime.UtcNow).TotalSeconds;
}
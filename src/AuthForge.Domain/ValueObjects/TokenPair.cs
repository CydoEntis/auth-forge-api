namespace AuthForge.Domain.ValueObjects;

public sealed record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt)
{
    public int ExpiresinSeconds => (int)(AccessTokenExpiresAt - DateTime.UtcNow).TotalSeconds;
}
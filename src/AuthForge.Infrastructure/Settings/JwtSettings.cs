namespace AuthForge.Infrastructure.Settings;

public class JwtSettings
{
    public required string Secret { get; init; }
    public int AccessTokenExpirationMinutes { get; init; } = 15;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
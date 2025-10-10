namespace AuthForge.Domain.Entities;

public class TenantSettings
{
    public int MaxUsers  { get; init; }
    public int AccessTokenExpirationMinutes { get; init; }
    public int RefreshTokenExpirationMinutes { get; init; }
    public bool RequireEmailVerification { get; init; }
    public bool AllowSocialLogins  { get; init; }
    public int MaxFailedLoginAttempts  { get; init; }
    public int LockoutDurationMinutes { get; init; }

    public static TenantSettings Default() => new()
    {
        MaxUsers = 100,
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationMinutes = 7,
        RequireEmailVerification = true,
        AllowSocialLogins = false,
        MaxFailedLoginAttempts = 5,
        LockoutDurationMinutes = 15,
    };
    
    public static TenantSettings Enterprise() => new()
    {
        MaxUsers = 10000,
        AccessTokenExpirationMinutes = 30,
        RefreshTokenExpirationMinutes = 30,
        RequireEmailVerification = true,
        AllowSocialLogins = true,
        MaxFailedLoginAttempts = 3,
        LockoutDurationMinutes = 30,
    };
}
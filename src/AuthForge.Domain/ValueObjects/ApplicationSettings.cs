namespace AuthForge.Domain.ValueObjects;

public sealed record ApplicationSettings
{
    public int MaxFailedLoginAttempts { get; init; }
    public int LockoutDurationMinutes { get; init; }
    public int AccessTokenExpirationMinutes { get; init; }
    public int RefreshTokenExpirationDays { get; init; }

    private ApplicationSettings(
        int maxFailedLoginAttempts,
        int lockoutDurationMinutes,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays)
    {
        MaxFailedLoginAttempts = maxFailedLoginAttempts;
        LockoutDurationMinutes = lockoutDurationMinutes;
        AccessTokenExpirationMinutes = accessTokenExpirationMinutes;
        RefreshTokenExpirationDays = refreshTokenExpirationDays;
    }

    public static ApplicationSettings Default() => new(
        maxFailedLoginAttempts: 5,
        lockoutDurationMinutes: 15,
        accessTokenExpirationMinutes: 15,
        refreshTokenExpirationDays: 7);

    public static ApplicationSettings Create(
        int maxFailedLoginAttempts,
        int lockoutDurationMinutes,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays)
    {
        if (maxFailedLoginAttempts <= 0 || maxFailedLoginAttempts > 10)
            throw new ArgumentException(
                "Max failed login attempts must be between 1 and 10.",
                nameof(maxFailedLoginAttempts));

        if (lockoutDurationMinutes <= 0 || lockoutDurationMinutes > 1440)
            throw new ArgumentException(
                "Lockout duration must be between 1 and 1440 minutes.",
                nameof(lockoutDurationMinutes));

        if (accessTokenExpirationMinutes <= 0 || accessTokenExpirationMinutes > 1440)
            throw new ArgumentException(
                "Access token expiration must be between 1 and 1440 minutes.",
                nameof(accessTokenExpirationMinutes));

        if (refreshTokenExpirationDays <= 0 || refreshTokenExpirationDays > 90)
            throw new ArgumentException(
                "Refresh token expiration must be between 1 and 90 days.",
                nameof(refreshTokenExpirationDays));

        return new ApplicationSettings(
            maxFailedLoginAttempts,
            lockoutDurationMinutes,
            accessTokenExpirationMinutes,
            refreshTokenExpirationDays);
    }
}
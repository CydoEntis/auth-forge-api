namespace AuthForge.Domain.ValueObjects;

public sealed record ApplicationSettings(
    int MaxFailedLoginAttempts,
    int LockoutDurationMinutes,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays)
{
    public static ApplicationSettings Default() => new(
        MaxFailedLoginAttempts: 5,
        LockoutDurationMinutes: 15,
        AccessTokenExpirationMinutes: 15,
        RefreshTokenExpirationDays: 7
    );

    public ApplicationSettings Validate()
    {
        if (MaxFailedLoginAttempts <= 0)
            throw new ArgumentException("Max failed login attempts must be greater than 0.");

        if (LockoutDurationMinutes <= 0)
            throw new ArgumentException("Lockout duration must be greater than 0.");

        if (AccessTokenExpirationMinutes <= 0)
            throw new ArgumentException("Access token expiration must be greater than 0.");

        if (RefreshTokenExpirationDays <= 0)
            throw new ArgumentException("Refresh token expiration must be greater than 0.");

        return this;
    }
}
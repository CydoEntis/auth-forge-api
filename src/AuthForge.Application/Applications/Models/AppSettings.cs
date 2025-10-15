namespace AuthForge.Application.Applications.Models;

public sealed record AppSettings(
    int MaxFailedLoginAttempts,
    int LockoutDurationMinutes,
    int AccessTokenExpirationMinutes,
    int RefreshTokenExpirationDays);
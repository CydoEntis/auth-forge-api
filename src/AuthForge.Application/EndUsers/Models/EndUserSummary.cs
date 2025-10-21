namespace AuthForge.Application.EndUsers.Models;

public sealed record EndUserSummary(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    bool IsEmailVerified,
    bool IsActive,
    int FailedLoginAttempts,
    DateTime? LockedOutUntil,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);
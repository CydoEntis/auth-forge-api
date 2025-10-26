namespace AuthForge.Application.Admin.Queries.GetCurrentAdmin;

public sealed record GetCurrentAdminResponse(
    Guid Id,
    string Email,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc,
    int FailedLoginAttempts,
    DateTime? LockedOutUntil);
namespace AuthForge.Application.Applications.Queries.GetApplicationUserById;

public record GetApplicationUserByIdResponse(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    bool IsEmailVerified,
    bool IsActive,
    int FailedLoginAttempts,
    DateTime? LockedOutUntil,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? LastLoginAtUtc);
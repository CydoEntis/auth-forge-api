namespace AuthForge.Application.EndUsers.Queries.GetById;

public record GetEndUserByIdResponse(
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
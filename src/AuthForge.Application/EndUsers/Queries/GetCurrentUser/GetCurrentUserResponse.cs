namespace AuthForge.Application.EndUsers.Queries.GetCurrentUser;

public record GetCurrentUserResponse(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    bool IsEmailVerified,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);
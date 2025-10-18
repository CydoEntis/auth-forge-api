namespace AuthForge.Application.EndUsers.Models;

public sealed record EndUserSummary(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    bool EmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
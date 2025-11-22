namespace AuthForge.Api.Features.Users.Shared.Models;

public sealed record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified,
    DateTime? LastLoginAtUtc,
    DateTime CreatedAtUtc
);
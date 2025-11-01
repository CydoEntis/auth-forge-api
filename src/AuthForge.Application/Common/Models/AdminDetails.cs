namespace AuthForge.Application.Common.Models;

public record AdminDetails(
    Guid Id,
    string Email,
    DateTime CreatedAt);
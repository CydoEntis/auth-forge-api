namespace AuthForge.Application.Admin.Commands.SetUpAdmin;

public record AdminDetails(
    Guid Id,
    string Email,
    DateTime CreatedAt);
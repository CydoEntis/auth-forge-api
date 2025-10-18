namespace AuthForge.Application.Applications.Commands.Create;

public sealed record CreateApplicationResponse(
    string ApplicationId,
    string Name,
    string Slug,
    bool IsActive,
    DateTime CreatedAtUtc);
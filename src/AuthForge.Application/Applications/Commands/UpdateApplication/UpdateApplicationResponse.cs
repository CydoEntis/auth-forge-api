namespace AuthForge.Application.Applications.Commands.UpdateApplication;

public sealed record UpdateApplicationResponse(
    string ApplicationId,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    DateTime UpdatedAtUtc);
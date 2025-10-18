namespace AuthForge.Application.Applications.Commands.Update;

public sealed record UpdateApplicationResponse(
    string ApplicationId,
    string Name,
    string Slug,
    bool IsActive,
    DateTime UpdatedAtUtc);
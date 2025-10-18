namespace AuthForge.Application.Applications.Commands.Create;

public sealed record CreateApplicationResponse(
    string ApplicationId,
    string Name,
    string Slug,
    string PublicKey,
    string SecretKey,
    bool IsActive,
    DateTime CreatedAtUtc);
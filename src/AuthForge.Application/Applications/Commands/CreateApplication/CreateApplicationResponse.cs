namespace AuthForge.Application.Applications.Commands.CreateApplication;

public sealed record CreateApplicationResponse(
    string ApplicationId,
    string Name,
    string Slug,
    string? Description,
    string PublicKey,
    string SecretKey,
    string JwtSecret,
    List<string> AllowedOrigins,
    bool IsActive,
    DateTime CreatedAtUtc);
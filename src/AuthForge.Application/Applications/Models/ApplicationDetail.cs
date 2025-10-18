namespace AuthForge.Application.Applications.Models;

public sealed record ApplicationDetail(
    string ApplicationId,
    string Name,
    string Slug,
    string PublicKey, 
    bool IsActive,
    AppSettings Settings,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? DeactivatedAtUtc);
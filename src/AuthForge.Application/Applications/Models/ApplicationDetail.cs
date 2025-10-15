namespace AuthForge.Application.Applications.Models;

public sealed record ApplicationDetail(
    string ApplicationId,
    string Name,
    string Slug,
    bool IsActive,
    AppSettings Settings,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? DeactivatedAtUtc);
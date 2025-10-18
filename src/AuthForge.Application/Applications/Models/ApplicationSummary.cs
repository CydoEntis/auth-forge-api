namespace AuthForge.Application.Applications.Models;

public sealed record ApplicationSummary(
    string ApplicationId,
    string Name,
    string Slug,
    bool IsActive,
    int EndUserCount, 
    DateTime CreatedAtUtc);
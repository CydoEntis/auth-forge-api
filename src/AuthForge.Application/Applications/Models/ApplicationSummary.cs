namespace AuthForge.Application.Applications.Models;

public sealed record ApplicationSummary(
    string ApplicationId,
    string Name,
    string Slug,
    string PublicKey, 
    bool IsActive,
    int EndUserCount, 
    DateTime CreatedAtUtc);
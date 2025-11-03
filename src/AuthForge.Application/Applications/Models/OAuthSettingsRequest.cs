namespace AuthForge.Application.Applications.Models;

public record OAuthSettingsRequest(
    bool GoogleEnabled,
    string? GoogleClientId,
    string? GoogleClientSecret,
    bool GithubEnabled,
    string? GithubClientId,
    string? GithubClientSecret);
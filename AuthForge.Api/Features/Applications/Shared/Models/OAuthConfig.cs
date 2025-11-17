namespace AuthForge.Api.Features.Applications.Shared.Models;

public record OAuthConfig(
    bool Enabled,
    string? ClientId,
    string? ClientSecret);
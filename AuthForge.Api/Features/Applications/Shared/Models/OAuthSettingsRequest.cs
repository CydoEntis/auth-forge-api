namespace AuthForge.Api.Features.Applications.Shared.Models;

public record OAuthSettingsRequest(
    OAuthConfig Google,
    OAuthConfig Github
);
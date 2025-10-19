namespace AuthForge.Application.Common.Settings;

public class AuthForgeSettings
{
    public const string SectionName = "AuthForge";

    public required AdminSettings Admin { get; init; }
    public required JwtSettings Jwt { get; init; }
    public OAuthSettings? OAuth { get; init; }
}
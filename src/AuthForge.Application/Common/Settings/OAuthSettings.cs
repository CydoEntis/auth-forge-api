namespace AuthForge.Application.Common.Settings;

public class OAuthSettings
{
    public GoogleOAuthSettings? Google { get; init; }
    public GitHubOAuthSettings? GitHub { get; init; }
}
namespace AuthForge.Infrastructure.Settings;

public class OAuthSettings
{
    public GoogleOAuthSettings? Google { get; init; }
    public GitHubOAuthSettings? GitHub { get; init; }
}
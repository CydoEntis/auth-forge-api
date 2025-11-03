namespace AuthForge.Domain.ValueObjects;

public sealed record OAuthSettings
{
    public bool GoogleEnabled { get; private set; }
    public string? GoogleClientId { get; private set; }
    public string? GoogleClientSecret { get; private set; }
    
    public bool GithubEnabled { get; private set; }
    public string? GithubClientId { get; private set; }
    public string? GithubClientSecret { get; private set; }

    private OAuthSettings() { } 

    private OAuthSettings(
        bool googleEnabled,
        string? googleClientId,
        string? googleClientSecret,
        bool githubEnabled,
        string? githubClientId,
        string? githubClientSecret)
    {
        GoogleEnabled = googleEnabled;
        GoogleClientId = googleClientId;
        GoogleClientSecret = googleClientSecret;
        GithubEnabled = githubEnabled;
        GithubClientId = githubClientId;
        GithubClientSecret = githubClientSecret;
    }

    public static OAuthSettings Create(
        bool googleEnabled = false,
        string? googleClientId = null,
        string? googleClientSecret = null,
        bool githubEnabled = false,
        string? githubClientId = null,
        string? githubClientSecret = null)
    {
        if (googleEnabled)
        {
            if (string.IsNullOrWhiteSpace(googleClientId))
                throw new ArgumentException("Google Client ID is required when Google OAuth is enabled", nameof(googleClientId));

            if (string.IsNullOrWhiteSpace(googleClientSecret))
                throw new ArgumentException("Google Client Secret is required when Google OAuth is enabled", nameof(googleClientSecret));
        }

        if (githubEnabled)
        {
            if (string.IsNullOrWhiteSpace(githubClientId))
                throw new ArgumentException("Github Client ID is required when Github OAuth is enabled", nameof(githubClientId));

            if (string.IsNullOrWhiteSpace(githubClientSecret))
                throw new ArgumentException("Github Client Secret is required when Github OAuth is enabled", nameof(githubClientSecret));
        }

        return new OAuthSettings(
            googleEnabled,
            googleClientId,
            googleClientSecret,
            githubEnabled,
            githubClientId,
            githubClientSecret);
    }
}
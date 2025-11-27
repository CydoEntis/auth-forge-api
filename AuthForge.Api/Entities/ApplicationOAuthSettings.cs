namespace AuthForge.Api.Entities;

public class ApplicationOAuthSettings
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    public bool GoogleEnabled { get; set; }
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecretEncrypted { get; set; }

    public bool GithubEnabled { get; set; }
    public string? GithubClientId { get; set; }
    public string? GithubClientSecretEncrypted { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Application Application { get; set; } = null!;
}
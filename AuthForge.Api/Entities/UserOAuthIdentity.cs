namespace AuthForge.Api.Entities;

public class UserOAuthIdentity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = null!;
    public string ProviderUserId { get; set; } = null!;
    public string? ProviderEmail { get; set; }
    public string? ProviderDisplayName { get; set; }
    public string? ProviderAvatarUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}
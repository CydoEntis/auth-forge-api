namespace AuthForge.Api.Entities;

public class AdminPasswordResetToken
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    
    public Admin Admin { get; set; } = null!;
}
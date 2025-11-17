namespace AuthForge.Api.Entities;

public class AdminRefreshToken
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    
    public Admin Admin { get; set; } = null!;
}
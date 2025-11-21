namespace AuthForge.Api.Entities;

public class UserRefreshToken
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public User User { get; set; } = null!;
}
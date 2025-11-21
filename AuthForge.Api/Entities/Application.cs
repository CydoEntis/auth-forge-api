namespace AuthForge.Api.Entities;

public class Application
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    
    public string ClientId { get; set; } = null!;
    public string ClientSecretEncrypted { get; set; } = null!;
    public string JwtSecretEncrypted { get; set; } = null!;
    

    public List<string> RedirectUris { get; set; } = new();
    public List<string> PostLogoutRedirectUris { get; set; } = new();
    
    public List<string> AllowedOrigins { get; set; } = new();
    public bool IsActive { get; set; }

    public int MaxFailedLoginAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }

    public bool UseGlobalEmailSettings { get; set; } = true;
    public string? EmailProvider { get; set; }
    public string? EmailApiKeyEncrypted { get; set; }
    
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? PasswordResetCallbackUrl { get; set; }
    public string? EmailVerificationCallbackUrl { get; set; }

    public bool GoogleEnabled { get; set; }
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecretEncrypted { get; set; }
    
    public bool GithubEnabled { get; set; }
    public string? GithubClientId { get; set; }
    public string? GithubClientSecretEncrypted { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    
    public ICollection<User> Users { get; set; } = new List<User>();
}
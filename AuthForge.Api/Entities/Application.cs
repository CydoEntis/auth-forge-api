namespace AuthForge.Api.Entities;

public class Application
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string PublicKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!; // Stored encrypted
    public string JwtSecret { get; set; } = null!; // Stored encrypted
    public List<string> AllowedOrigins { get; set; } = new();
    public bool IsActive { get; set; }

    public int MaxFailedLoginAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }

    public string? EmailProvider { get; set; }
    public string? EmailApiKey { get; set; } // Stored encrypted
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? PasswordResetCallbackUrl { get; set; }
    public string? EmailVerificationCallbackUrl { get; set; }

    public bool GoogleEnabled { get; set; }
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; } // Stored encrypted
    public bool GithubEnabled { get; set; }
    public string? GithubClientId { get; set; }
    public string? GithubClientSecret { get; set; } // Stored encrypted

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
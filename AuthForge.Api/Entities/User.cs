namespace AuthForge.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public bool EmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedOutUntil { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }


    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Application Application { get; set; } = null!;
    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();
    public ICollection<UserPasswordResetToken> PasswordResetTokens { get; set; } = new List<UserPasswordResetToken>();
    public ICollection<UserOAuthIdentity> OAuthIdentities { get; set; } = new List<UserOAuthIdentity>();
}
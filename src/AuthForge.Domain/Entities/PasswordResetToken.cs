using System.Security.Cryptography;
using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Entities;


public sealed class PasswordResetToken : Entity<Guid>
{
    private PasswordResetToken() { }

    private PasswordResetToken(
        Guid id,
        UserId userId,
        string token,
        DateTime expiresAtUtc) : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        IsUsed = false;
    }

    public UserId UserId { get; private set; } = default!;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsValid => !IsUsed && !IsExpired;

    public static PasswordResetToken Create(
        UserId userId,
        int expirationHours = 1)
    {
        if (expirationHours <= 0)
            throw new ArgumentException("Expiration hours must be positive", nameof(expirationHours));

        string token = GenerateSecureToken();
        DateTime expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        return new PasswordResetToken(
            Guid.NewGuid(),
            userId,
            token,
            expiresAt);
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Token has already been used");

        if (IsExpired)
            throw new InvalidOperationException("Token has expired");

        IsUsed = true;
        UsedAtUtc = DateTime.UtcNow;
    }
    
    private static string GenerateSecureToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
        
        return Convert.ToBase64String(randomBytes);
    }
}
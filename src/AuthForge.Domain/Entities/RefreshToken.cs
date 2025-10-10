using System.Security.Cryptography;
using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    private RefreshToken() { }

    private RefreshToken(
        Guid id,
        UserId userId,
        string token,
        DateTime expiresAtUtc,
        string? ipAddress = null,
        string? userAgent = null) : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public UserId UserId { get; private set; } = default!;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static RefreshToken Create(
        UserId userId,
        int expirationDays,
        string? ipAddress = null,
        string? userAgent = null)
    {
        string token = GenerateSecureToken();
        DateTime expiresAt = DateTime.UtcNow.AddDays(expirationDays);

        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            token,
            expiresAt,
            ipAddress,
            userAgent);
    }

    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token is already revoked");

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }

    public void MarkAsUsed()
    {
        if (UsedAtUtc.HasValue)
            throw new InvalidOperationException("Token has already been used");

        UsedAtUtc = DateTime.UtcNow;
    }

    private static string GenerateSecureToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        
        return Convert.ToBase64String(randomBytes);
    }
}
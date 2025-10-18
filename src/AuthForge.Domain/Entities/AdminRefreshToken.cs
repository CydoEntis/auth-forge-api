using AuthForge.Domain.Common;

namespace AuthForge.Domain.Entities;

public sealed class AdminRefreshToken : Entity<Guid>
{
    private AdminRefreshToken()
    {
    }

    private AdminRefreshToken(
        Guid id,
        string token,
        DateTime expiresAtUtc,
        string? ipAddress = null,
        string? userAgent = null) : base(id)
    {
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

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

    public static AdminRefreshToken Create(
        string token,
        DateTime expiresAtUtc,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAtUtc));

        return new AdminRefreshToken(
            Guid.NewGuid(),
            token,
            expiresAtUtc,
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
}
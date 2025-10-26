using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Entities;

public sealed class Admin : AggregateRoot<AdminId>
{
    private Admin()
    {
    }

    private Admin(
        AdminId id,
        Email email,
        HashedPassword passwordHash
    ) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        IsEmailVerified = false;
        FailedLoginAttempts = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Email Email { get; private set; }
    public HashedPassword PasswordHash { get; private set; } = default!;
    public bool IsEmailVerified { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedOutUntil { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public static Admin Create(
        Email email,
        HashedPassword passwordHash)
    {
        return new Admin(
            AdminId.CreateUnique(),
            email,
            passwordHash);
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
        LastLoginAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RecordFailedLogin(int maxAttempts, int lockoutMinutes)
    {
        FailedLoginAttempts++;
        UpdatedAtUtc = DateTime.UtcNow;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedOutUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    public bool IsLockedOut()
    {
        return LockedOutUntil.HasValue && LockedOutUntil.Value > DateTime.UtcNow;
    }

    public void Unlock()
    {
        LockedOutUntil = null;
        FailedLoginAttempts = 0;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePassword(HashedPassword newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetPasswordResetToken(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Token expiration must be in the future.", nameof(expiresAt));

        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = expiresAt;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool IsPasswordResetTokenValid(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return PasswordResetToken == token &&
               PasswordResetTokenExpiresAt.HasValue &&
               PasswordResetTokenExpiresAt.Value > DateTime.UtcNow;
    }
    
    public void ClearExpiredPasswordResetToken()
    {
        if (PasswordResetToken != null && 
            PasswordResetTokenExpiresAt.HasValue && 
            DateTime.UtcNow > PasswordResetTokenExpiresAt.Value)
        {
            PasswordResetToken = null;
            PasswordResetTokenExpiresAt = null;
        }
    }
}
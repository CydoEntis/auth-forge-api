using AuthForge.Domain.Common;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Entities;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<RefreshToken> _refreshTokens = new();

    private User()
    {
    }

    private User(
        UserId id,
        TenantId tenantId,
        Email email,
        HashedPassword password,
        string firstName,
        string lastName) : base(id)
    {
        TenantId = tenantId;
        Email = email;
        PasswordHash = password;
        FirstName = firstName;
        LastName = lastName;
        IsEmailVerified = false;
        IsActive = true;
        FailedLoginAttempts = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public TenantId TenantId { get; private set; }
    public Email Email { get; private set; }
    public HashedPassword PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedOutUntil { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public string FullName => $"{FirstName} {LastName}";

    public static User Create(TenantId tenantId, Email email, HashedPassword password, string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        var user = new User(
            UserId.CreateUnique(),
            tenantId,
            email,
            password,
            firstName,
            lastName);

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, user.TenantId, user.Email, user.FirstName,
            user.LastName));

        return user;
    }

    public void VerifyEmail()
    {
        if (IsEmailVerified)
            throw new InvalidOperationException("Email is already verified.");

        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;

        RaiseDomainEvent(new UserEmailVerifiedDomainEvent(Id, Email));
    }

    public void SetEmailVerificationToken(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Token expiration must be in the future.", nameof(expiresAt));

        EmailVerificationToken = token;
        EmailVerificationTokenExpiresAt = expiresAt;
    }

    public bool IsEmailVerificationTokenValid(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return EmailVerificationToken == token &&
               EmailVerificationTokenExpiresAt.HasValue &&
               EmailVerificationTokenExpiresAt.Value > DateTime.UtcNow;
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
        LastLoginAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new UserLoggedInDomainEvent(Id, TenantId, Email));
    }

    public void RecordFailedLogin(int maxAttempts, int lockoutMinutes)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedOutUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            RaiseDomainEvent(new UserLockedOutDomainEvent(Id, LockedOutUntil.Value, FailedLoginAttempts));
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
    }

    public void UpdatePassword(HashedPassword newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentException(nameof(newPasswordHash));
        UpdatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new UserPasswordChangedDomainEvent(Id));
    }

    public void Decativate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already deactivated.");

        IsActive = false;

        RevokeAllRefreshTokens();
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active.");

        IsActive = true;
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        if (refreshToken is null)
            throw new ArgumentNullException(nameof(refreshToken));

        _refreshTokens.Add(refreshToken);
    }

    public void RemoveOldRefreshTokens(int retentionDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
    
        _refreshTokens.RemoveAll(t => 
            !t.IsActive && t.CreatedAtUtc < cutoffDate);
    }
    
    public void RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.Revoke();
            RaiseDomainEvent(new RefreshTokenRevokedDomainEvent(Id, token));
        }
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoke();
        }
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        UpdatedAtUtc = DateTime.UtcNow; 
    }

    public void UpdateTimestamp()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
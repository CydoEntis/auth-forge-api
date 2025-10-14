using AuthForge.Domain.Common;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Entities;

public sealed class EndUser : AggregateRoot<EndUserId>
{
    private EndUser()
    {
    }

    private EndUser(
        EndUserId id,
        ApplicationId applicationId,
        Email email,
        HashedPassword password,
        string firstName,
        string lastName) : base(id)
    {
        ApplicationId = applicationId;
        Email = email;
        PasswordHash = password;
        FirstName = firstName;
        LastName = lastName;
        IsEmailVerified = false;
        IsActive = true;
        FailedLoginAttempts = 0;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public ApplicationId ApplicationId { get; private set; } = default!;
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

    public string FullName => $"{FirstName} {LastName}";

    public static EndUser Create(
        ApplicationId applicationId,
        Email email,
        HashedPassword password,
        string firstName,
        string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        var user = new EndUser(
            EndUserId.CreateUnique(),
            applicationId,
            email,
            password,
            firstName,
            lastName);

        user.RaiseDomainEvent(new EndUserRegisteredDomainEvent(
            user.Id,
            user.ApplicationId,
            user.Email,
            user.FirstName,
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

        RaiseDomainEvent(new EndUserEmailVerifiedDomainEvent(Id, Email));
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

        RaiseDomainEvent(new EndUserLoggedInDomainEvent(Id, ApplicationId, Email));
    }

    public void RecordFailedLogin(int maxAttempts, int lockoutMinutes)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedOutUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            RaiseDomainEvent(new EndUserLockedOutDomainEvent(Id, LockedOutUntil.Value, FailedLoginAttempts));
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
        RaiseDomainEvent(new EndUserPasswordChangedDomainEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already deactivated.");

        IsActive = false;
        RaiseDomainEvent(new EndUserDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active.");

        IsActive = true;
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
}
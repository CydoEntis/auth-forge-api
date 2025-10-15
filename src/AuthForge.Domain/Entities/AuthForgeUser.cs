using AuthForge.Domain.Common;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Entities;

// This is the Developer Account (User that will sign up to Auth Forge)
public sealed class AuthForgeUser : AggregateRoot<AuthForgeUserId>
{
    private AuthForgeUser()
    {
    }

    private AuthForgeUser(AuthForgeUserId id, Email email, HashedPassword hashedPassword, string firstName,
        string lastName) : base(id)
    {
        Email = email;
        HashedPassword = hashedPassword;
        FirstName = firstName;
        LastName = lastName;
        IsEmailVerified = false;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Email Email { get; private set; }
    public HashedPassword HashedPassword { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }
    public string FullName => $"{FirstName} {LastName}";

    public static AuthForgeUser Create(Email email, HashedPassword password, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));

        var user = new AuthForgeUser(
            AuthForgeUserId.CreateUnique(),
            email,
            password,
            firstName,
            lastName
        );

        user.RaiseDomainEvent(
            new AuthForgeUserRegisteredDomainEvent(user.Id, user.Email, user.FirstName, user.LastName));

        return user;
    }

    public void VerifyEmail()
    {
        if (!IsEmailVerified)
            throw new InvalidOperationException("Email is not verified");

        IsEmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;

        RaiseDomainEvent(new AuthForgeUserEmailVerifiedDomainEvent(Id, Email));
    }

    public void SetEmailVerificationToken(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Token expiration must be in the future", nameof(expiresAt));

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
        LastLoginAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new AuthForgeUserLoggedInDomainEvent(Id, Email));
    }

    public void UpdatePassword(HashedPassword newHashedPassword)
    {
        HashedPassword = newHashedPassword ?? throw new ArgumentException(nameof(newHashedPassword));
        UpdatedAtUtc = DateTime.UtcNow;
        RaiseDomainEvent(new AuthForgeUserPasswordChangedDomainEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("User is already deactivated");

        IsActive = false;
        RaiseDomainEvent(new AuthForgeUserDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("User is already active");

        IsActive = false;
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
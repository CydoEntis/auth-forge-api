using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Tests.Entities;

public class EndUserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateEndUser()
    {
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var password = HashedPassword.FromHash("hash", "salt");
        var firstName = "Jane";
        var lastName = "Smith";

        var endUser = EndUser.Create(applicationId, email, password, firstName, lastName);

        endUser.Should().NotBeNull();
        endUser.ApplicationId.Should().Be(applicationId);
        endUser.Email.Should().Be(email);
        endUser.PasswordHash.Should().Be(password);
        endUser.FirstName.Should().Be(firstName);
        endUser.LastName.Should().Be(lastName);
        endUser.IsActive.Should().BeTrue();
        endUser.IsEmailVerified.Should().BeFalse();
        endUser.FailedLoginAttempts.Should().Be(0);
        endUser.IsLockedOut().Should().BeFalse();
        endUser.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void VerifyEmail_WhenNotVerified_ShouldVerifyEmail()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        endUser.VerifyEmail();

        endUser.IsEmailVerified.Should().BeTrue();
        endUser.EmailVerificationToken.Should().BeNull();
        endUser.EmailVerificationTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ShouldThrowException()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        endUser.VerifyEmail();

        Action act = () => endUser.VerifyEmail();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already verified*");
    }

    [Fact]
    public void SetEmailVerificationToken_WithValidToken_ShouldSetToken()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        var token = "verification-token";
        var expiresAt = DateTime.UtcNow.AddHours(24);

        endUser.SetEmailVerificationToken(token, expiresAt);

        endUser.EmailVerificationToken.Should().Be(token);
        endUser.EmailVerificationTokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void IsEmailVerificationTokenValid_WithValidToken_ShouldReturnTrue()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        var token = "verification-token";
        endUser.SetEmailVerificationToken(token, DateTime.UtcNow.AddHours(24));

        var isValid = endUser.IsEmailVerificationTokenValid(token);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementAttempts()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        endUser.RecordFailedLogin(5, 15);

        endUser.FailedLoginAttempts.Should().Be(1);
        endUser.IsLockedOut().Should().BeFalse();
    }

    [Fact]
    public void RecordFailedLogin_WhenMaxAttemptsReached_ShouldLockOut()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        for (int i = 0; i < 5; i++)
        {
            endUser.RecordFailedLogin(5, 15);
        }

        endUser.FailedLoginAttempts.Should().Be(5);
        endUser.IsLockedOut().Should().BeTrue();
        endUser.LockedOutUntil.Should().NotBeNull();
        endUser.LockedOutUntil.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Unlock_WhenLockedOut_ShouldUnlock()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        for (int i = 0; i < 5; i++)
        {
            endUser.RecordFailedLogin(5, 15);
        }

        endUser.Unlock();

        endUser.IsLockedOut().Should().BeFalse();
        endUser.LockedOutUntil.Should().BeNull();
        endUser.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePassword()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("old-hash", "old-salt"),
            "Jane",
            "Smith");
        var newPassword = HashedPassword.FromHash("new-hash", "new-salt");

        endUser.UpdatePassword(newPassword);

        endUser.PasswordHash.Should().Be(newPassword);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateUser()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        endUser.Deactivate();

        endUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowException()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        endUser.Deactivate();

        Action act = () => endUser.Deactivate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already deactivated*");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateUser()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        endUser.Deactivate();

        endUser.Activate();

        endUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowException()
    {
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        Action act = () => endUser.Activate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already active*");
    }
}
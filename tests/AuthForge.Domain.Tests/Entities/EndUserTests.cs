using AuthForge.Domain.Entities;
using AuthForge.Domain.Events;
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

    [Fact]
    public void RecordFailedLogin_ShouldRaiseLoginFailedDomainEvent()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        // Clear initial domain events from Create
        endUser.ClearDomainEvents();

        // ACT
        endUser.RecordFailedLogin(5, 15);

        // ASSERT
        var domainEvents = endUser.DomainEvents;
        domainEvents.Should().ContainSingle(e => e is EndUserLoginFailedDomainEvent);

        var loginFailedEvent = domainEvents.OfType<EndUserLoginFailedDomainEvent>().Single();
        loginFailedEvent.UserId.Should().Be(endUser.Id);
        loginFailedEvent.ApplicationId.Should().Be(applicationId);
        loginFailedEvent.Email.Should().Be(email);
        loginFailedEvent.FailedAttempts.Should().Be(1);
    }

    [Fact]
    public void RecordFailedLogin_WithMultipleAttempts_ShouldRaiseEventWithCorrectCount()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        endUser.ClearDomainEvents();

        // ACT
        endUser.RecordFailedLogin(5, 15);
        endUser.RecordFailedLogin(5, 15);
        endUser.RecordFailedLogin(5, 15);

        // ASSERT
        var domainEvents = endUser.DomainEvents;
        var loginFailedEvents = domainEvents.OfType<EndUserLoginFailedDomainEvent>().ToList();

        loginFailedEvents.Should().HaveCount(3);
        loginFailedEvents[0].FailedAttempts.Should().Be(1);
        loginFailedEvents[1].FailedAttempts.Should().Be(2);
        loginFailedEvents[2].FailedAttempts.Should().Be(3);
    }

    [Fact]
    public void RecordFailedLogin_WhenReachingMaxAttempts_ShouldRaiseBothLoginFailedAndLockedOutEvents()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        endUser.ClearDomainEvents();

        // ACT - Record 5 failed attempts (max is 5)
        for (int i = 0; i < 5; i++)
        {
            endUser.RecordFailedLogin(5, 15);
        }

        // ASSERT
        var domainEvents = endUser.DomainEvents;

        // Should have 5 LoginFailed events and 1 LockedOut event
        var loginFailedEvents = domainEvents.OfType<EndUserLoginFailedDomainEvent>().ToList();
        var lockedOutEvents = domainEvents.OfType<EndUserLockedOutDomainEvent>().ToList();

        loginFailedEvents.Should().HaveCount(5);
        lockedOutEvents.Should().HaveCount(1);

        // The last LoginFailed event should have the max count
        loginFailedEvents.Last().FailedAttempts.Should().Be(5);

        // The LockedOut event should also reflect the failed attempts
        lockedOutEvents.Single().FailedAttempts.Should().Be(5);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(3, 10)]
    [InlineData(7, 20)]
    public void RecordFailedLogin_ShouldRaiseEventWithCorrectFailedAttemptsCount(int attempts, int maxAttempts)
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        endUser.ClearDomainEvents();

        // ACT
        for (int i = 0; i < attempts; i++)
        {
            endUser.RecordFailedLogin(maxAttempts, 15);
        }

        // ASSERT
        var domainEvents = endUser.DomainEvents;
        var loginFailedEvents = domainEvents.OfType<EndUserLoginFailedDomainEvent>().ToList();

        loginFailedEvents.Should().HaveCount(attempts);
        loginFailedEvents.Last().FailedAttempts.Should().Be(attempts);
    }

    [Fact]
    public void RecordSuccessfulLogin_ShouldRaiseLoggedInDomainEvent()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        endUser.ClearDomainEvents();

        // ACT
        endUser.RecordSuccessfulLogin();

        // ASSERT
        var domainEvents = endUser.DomainEvents;
        domainEvents.Should().ContainSingle(e => e is EndUserLoggedInDomainEvent);

        var loggedInEvent = domainEvents.OfType<EndUserLoggedInDomainEvent>().Single();
        loggedInEvent.UserId.Should().Be(endUser.Id);
        loggedInEvent.ApplicationId.Should().Be(applicationId);
        loggedInEvent.Email.Should().Be(email);
    }

    [Fact]
    public void RecordSuccessfulLogin_ShouldResetFailedLoginAttempts()
    {
        // ARRANGE
        var endUser = EndUser.Create(
            ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        // Record some failed attempts
        endUser.RecordFailedLogin(5, 15);
        endUser.RecordFailedLogin(5, 15);

        // ACT
        endUser.RecordSuccessfulLogin();

        // ASSERT
        endUser.FailedLoginAttempts.Should().Be(0);
        endUser.LockedOutUntil.Should().BeNull();
        endUser.LastLoginAtUtc.Should().NotBeNull();
        endUser.LastLoginAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
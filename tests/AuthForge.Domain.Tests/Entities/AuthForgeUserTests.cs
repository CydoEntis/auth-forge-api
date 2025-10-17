using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthForge.Domain.Tests.Entities;

public class AuthForgeUserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        var email = Email.Create("test@example.com");
        var password = HashedPassword.FromHash("hash", "salt");
        var firstName = "John";
        var lastName = "Doe";

        var user = AuthForgeUser.Create(email, password, firstName, lastName);

        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.HashedPassword.Should().Be(password);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsActive.Should().BeTrue();
        user.IsEmailVerified.Should().BeFalse();
        user.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void VerifyEmail_WhenNotVerified_ShouldThrowException()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        Action act = () => user.VerifyEmail();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not verified*");
    }

    [Fact]
    public void SetEmailVerificationToken_WithValidToken_ShouldSetToken()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        var token = "verification-token";
        var expiresAt = DateTime.UtcNow.AddHours(24);

        user.SetEmailVerificationToken(token, expiresAt);

        user.EmailVerificationToken.Should().Be(token);
        user.EmailVerificationTokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void IsEmailVerificationTokenValid_WithValidToken_ShouldReturnTrue()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        var token = "verification-token";
        user.SetEmailVerificationToken(token, DateTime.UtcNow.AddHours(24));

        var isValid = user.IsEmailVerificationTokenValid(token);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsEmailVerificationTokenValid_WithInvalidToken_ShouldReturnFalse()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        user.SetEmailVerificationToken("correct-token", DateTime.UtcNow.AddHours(24));

        var isValid = user.IsEmailVerificationTokenValid("wrong-token");

        isValid.Should().BeFalse();
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePassword()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("old-hash", "old-salt"),
            "John",
            "Doe");
        var newPassword = HashedPassword.FromHash("new-hash", "new-salt");

        user.UpdatePassword(newPassword);

        user.HashedPassword.Should().Be(newPassword);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateUser()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowException()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        user.Deactivate();

        Action act = () => user.Deactivate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already deactivated*");
    }
}
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthForge.Domain.Tests.ValueObjects;

public class HashedPasswordTests
{
    [Fact]
    public void Create_WithValidPassword_ShouldCreateHashedPassword()
    {
        var plainTextPassword = "SecurePassword123!";

        var hashedPassword = HashedPassword.Create(plainTextPassword);

        hashedPassword.Should().NotBeNull();
        hashedPassword.Hash.Should().NotBeNullOrEmpty();
        hashedPassword.Salt.Should().NotBeNullOrEmpty();
        hashedPassword.Hash.Should().NotBe(plainTextPassword);
    }

    [Fact]
    public void Create_WithValidPassword_ShouldGenerateUniqueSalts()
    {
        var plainTextPassword = "SecurePassword123!";

        var hashedPassword1 = HashedPassword.Create(plainTextPassword);
        var hashedPassword2 = HashedPassword.Create(plainTextPassword);

        hashedPassword1.Salt.Should().NotBe(hashedPassword2.Salt);
        hashedPassword1.Hash.Should().NotBe(hashedPassword2.Hash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPassword_ShouldThrowException(string invalidPassword)
    {
        Action act = () => HashedPassword.Create(invalidPassword);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password cannot be empty*");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    [InlineData("Pass")]
    public void Create_WithPasswordTooShort_ShouldThrowException(string shortPassword)
    {
        Action act = () => HashedPassword.Create(shortPassword);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least 8 characters*");
    }

    [Fact]
    public void FromHash_WithValidHashAndSalt_ShouldCreateHashedPassword()
    {
        var hash = "dGVzdGhhc2g=";  
        var salt = "dGVzdHNhbHQ=";  

        var hashedPassword = HashedPassword.FromHash(hash, salt);

        hashedPassword.Should().NotBeNull();
        hashedPassword.Hash.Should().Be(hash);
        hashedPassword.Salt.Should().Be(salt);
    }

    [Theory]
    [InlineData("", "validSalt")]
    [InlineData("   ", "validSalt")]
    [InlineData(null, "validSalt")]
    public void FromHash_WithEmptyHash_ShouldThrowException(string invalidHash, string validSalt)
    {
        Action act = () => HashedPassword.FromHash(invalidHash, validSalt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Hash cannot be empty*");
    }

    [Theory]
    [InlineData("validHash", "")]
    [InlineData("validHash", "   ")]
    [InlineData("validHash", null)]
    public void FromHash_WithEmptySalt_ShouldThrowException(string validHash, string invalidSalt)
    {
        Action act = () => HashedPassword.FromHash(validHash, invalidSalt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Salt cannot be empty*");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var plainTextPassword = "MySecurePassword123!";
        var hashedPassword = HashedPassword.Create(plainTextPassword);

        var isValid = hashedPassword.Verify(plainTextPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        var correctPassword = "CorrectPassword123!";
        var incorrectPassword = "WrongPassword123!";
        var hashedPassword = HashedPassword.Create(correctPassword);

        var isValid = hashedPassword.Verify(incorrectPassword);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithCaseDifference_ShouldReturnFalse()
    {
        var password = "MyPassword123!";
        var hashedPassword = HashedPassword.Create(password);

        var isValid = hashedPassword.Verify("mypassword123!");

        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Verify_WithEmptyPassword_ShouldReturnFalse(string emptyPassword)
    {
        var hashedPassword = HashedPassword.Create("ValidPassword123!");

        var isValid = hashedPassword.Verify(emptyPassword);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        var password = "P@$$w0rd!#%&*()";
        var hashedPassword = HashedPassword.Create(password);

        var isValid = hashedPassword.Verify(password);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithUnicodeCharacters_ShouldWorkCorrectly()
    {
        var password = "Pässwörd123!中文";
        var hashedPassword = HashedPassword.Create(password);

        var isValid = hashedPassword.Verify(password);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMaximumLengthPassword_ShouldWorkCorrectly()
    {
        var password = new string('a', 100) + "123!";

        var hashedPassword = HashedPassword.Create(password);
        var isValid = hashedPassword.Verify(password);

        hashedPassword.Should().NotBeNull();
        isValid.Should().BeTrue();
    }
}

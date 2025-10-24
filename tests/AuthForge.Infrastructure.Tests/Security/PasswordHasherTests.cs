using AuthForge.Domain.ValueObjects;
using AuthForge.Infrastructure.Security;
using FluentAssertions;
using Xunit;

namespace AuthForge.Infrastructure.Tests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        var password = "MySecurePassword123!";

        var hashedPassword = _passwordHasher.HashPassword(password);

        hashedPassword.Should().NotBeNull();
        hashedPassword.Hash.Should().NotBeNullOrEmpty();
        hashedPassword.Salt.Should().NotBeNullOrEmpty();
        hashedPassword.Hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_CalledTwiceWithSamePassword_ShouldProduceDifferentHashes()
    {
        var password = "MySecurePassword123!";

        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        hash1.Salt.Should().NotBe(hash2.Salt);
        hash1.Hash.Should().NotBe(hash2.Hash);
    }

    [Fact]
    public void HashPassword_ShouldProduceBase64EncodedHash()
    {
        var password = "MySecurePassword123!";

        var hashedPassword = _passwordHasher.HashPassword(password);

        Action decodeHash = () => Convert.FromBase64String(hashedPassword.Hash);
        Action decodeSalt = () => Convert.FromBase64String(hashedPassword.Salt);

        decodeHash.Should().NotThrow();
        decodeSalt.Should().NotThrow();
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "MySecurePassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var correctPassword = "CorrectPassword123!";
        var incorrectPassword = "WrongPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(correctPassword);

        var isValid = _passwordHasher.VerifyPassword(incorrectPassword, hashedPassword);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithCaseDifference_ShouldReturnFalse()
    {
        var password = "MyPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValid = _passwordHasher.VerifyPassword("mypassword123!", hashedPassword);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        var password = "MyPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValid = _passwordHasher.VerifyPassword("", hashedPassword);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithSpecialCharacters_ShouldWorkCorrectly()
    {
        var password = "P@$$w0rd!#%&*()_+-=[]{}|;:',.<>?/";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithUnicodeCharacters_ShouldWorkCorrectly()
    {
        var password = "Pässwörd123!中文日本語";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithVeryLongPassword_ShouldWorkCorrectly()
    {
        var password = new string('a', 1000) + "123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithSlightlyDifferentPassword_ShouldReturnFalse()
    {
        var password = "MyPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var isValidWithExtraChar = _passwordHasher.VerifyPassword("MyPassword123!a", hashedPassword);
        var isValidWithMissingChar = _passwordHasher.VerifyPassword("MyPassword123", hashedPassword);

        isValidWithExtraChar.Should().BeFalse();
        isValidWithMissingChar.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_MultiplePasswords_ShouldProduceUniqueHashes()
    {
        var passwords = new[] { "Password1!", "Password2!", "Password3!" };
        var hashes = new List<HashedPassword>();

        foreach (var password in passwords)
        {
            hashes.Add(_passwordHasher.HashPassword(password));
        }

        hashes.Should().OnlyHaveUniqueItems(h => h.Hash);
        hashes.Should().OnlyHaveUniqueItems(h => h.Salt);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("password")]
    [InlineData("qwertyui")]
    public void VerifyPassword_WithCommonPasswords_ShouldStillWorkCorrectly(string commonPassword)
    {
        var hashedPassword = _passwordHasher.HashPassword(commonPassword);

        var isValid = _passwordHasher.VerifyPassword(commonPassword, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_UsesFixedTimeComparison()
    {
        var password = "MySecurePassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);
        var attempts = 100;
        var timings = new List<long>();

        for (int i = 0; i < attempts; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _passwordHasher.VerifyPassword("CompletelyDifferent!", hashedPassword);
            sw.Stop();
            timings.Add(sw.ElapsedTicks);
        }

        var avgWrongPassword = timings.Average();

        timings.Clear();
        for (int i = 0; i < attempts; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _passwordHasher.VerifyPassword("MySecurePassword123", hashedPassword);  
            sw.Stop();
            timings.Add(sw.ElapsedTicks);
        }

        var avgPartialMatch = timings.Average();

        var timingRatio = avgPartialMatch / avgWrongPassword;
        timingRatio.Should().BeInRange(0.5, 2.0, "fixed-time comparison should not leak password information");
    }
}

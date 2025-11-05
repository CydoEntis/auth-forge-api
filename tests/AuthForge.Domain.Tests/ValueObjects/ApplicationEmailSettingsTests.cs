using AuthForge.Domain.Enums;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthForge.Domain.Tests.ValueObjects;

public class ApplicationEmailSettingsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateSettings()
    {
        var provider = EmailProvider.Resend;
        var apiKey = "api_key_123";
        var fromEmail = "noreply@example.com";
        var fromName = "Example App";

        var settings = ApplicationEmailSettings.Create(provider, apiKey, fromEmail, fromName);

        settings.Should().NotBeNull();
        settings.Provider.Should().Be(provider);
        settings.ApiKey.Should().Be(apiKey);
        settings.FromEmail.Should().Be(fromEmail);
        settings.FromName.Should().Be(fromName);
        settings.PasswordResetCallbackUrl.Should().BeNull();
        settings.EmailVerificationCallbackUrl.Should().BeNull();
    }

    [Fact]
    public void Create_WithCallbackUrls_ShouldCreateSettings()
    {
        var provider = EmailProvider.Resend;
        var apiKey = "api_key_123";
        var fromEmail = "noreply@example.com";
        var fromName = "Example App";
        var passwordResetUrl = "https://example.com/reset-password";
        var emailVerificationUrl = "https://example.com/verify-email";

        var settings = ApplicationEmailSettings.Create(
            provider,
            apiKey,
            fromEmail,
            fromName,
            passwordResetUrl,
            emailVerificationUrl);

        settings.Should().NotBeNull();
        settings.PasswordResetCallbackUrl.Should().Be(passwordResetUrl);
        settings.EmailVerificationCallbackUrl.Should().Be(emailVerificationUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyApiKey_ShouldThrowException(string apiKey)
    {
        Action act = () => ApplicationEmailSettings.Create(
            EmailProvider.Resend,
            apiKey,
            "noreply@example.com",
            "Example App");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*API key cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFromEmail_ShouldThrowException(string fromEmail)
    {
        Action act = () => ApplicationEmailSettings.Create(
            EmailProvider.Resend,
            "api_key",
            fromEmail,
            "Example App");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*From email cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFromName_ShouldThrowException(string fromName)
    {
        Action act = () => ApplicationEmailSettings.Create(
            EmailProvider.Resend,
            "api_key",
            "noreply@example.com",
            fromName);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*From name cannot be empty*");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("http://")]
    [InlineData("://example.com")]
    [InlineData("www.example.com")]
    public void Create_WithInvalidPasswordResetUrl_ShouldThrowException(string invalidUrl)
    {
        Action act = () => ApplicationEmailSettings.Create(
            EmailProvider.Resend,
            "api_key",
            "noreply@example.com",
            "Example App",
            invalidUrl);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid password reset callback URL*");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("http://")]
    [InlineData("://example.com")]
    [InlineData("www.example.com")]
    public void Create_WithInvalidEmailVerificationUrl_ShouldThrowException(string invalidUrl)
    {
        Action act = () => ApplicationEmailSettings.Create(
            EmailProvider.Resend,
            "api_key",
            "noreply@example.com",
            "Example App",
            null,
            invalidUrl);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email verification callback URL*");
    }

    [Theory]
    [InlineData("https://example.com/reset")]
    [InlineData("http://localhost:3000/reset")]
    [InlineData("https://subdomain.example.com/reset-password")]
    public void Create_WithValidPasswordResetUrl_ShouldSucceed(string validUrl)
    {
        var settings = ApplicationEmailSettings.Create(
            EmailProvider.Resend,
            "api_key",
            "noreply@example.com",
            "Example App",
            validUrl);

        settings.PasswordResetCallbackUrl.Should().Be(validUrl);
    }

    [Theory]
    [InlineData("https://example.com/verify")]
    [InlineData("http://localhost:3000/verify")]
    [InlineData("https://subdomain.example.com/verify-email")]
    public void Create_WithValidEmailVerificationUrl_ShouldSucceed(string validUrl)
    {
        var settings = ApplicationEmailSettings.Create(
            EmailProvider.Resend,
            "api_key",
            "noreply@example.com",
            "Example App",
            null,
            validUrl);

        settings.EmailVerificationCallbackUrl.Should().Be(validUrl);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateCompleteSettings()
    {
        var provider = EmailProvider.Resend;
        var apiKey = "api_key_123";
        var fromEmail = "noreply@example.com";
        var fromName = "Example App";
        var passwordResetUrl = "https://example.com/reset";
        var emailVerificationUrl = "https://example.com/verify";

        var settings = ApplicationEmailSettings.Create(
            provider,
            apiKey,
            fromEmail,
            fromName,
            passwordResetUrl,
            emailVerificationUrl);

        settings.Provider.Should().Be(provider);
        settings.ApiKey.Should().Be(apiKey);
        settings.FromEmail.Should().Be(fromEmail);
        settings.FromName.Should().Be(fromName);
        settings.PasswordResetCallbackUrl.Should().Be(passwordResetUrl);
        settings.EmailVerificationCallbackUrl.Should().Be(emailVerificationUrl);
    }

    [Fact]
    public void EmailProvider_ResendValue_ShouldBeOne()
    {
        ((int)EmailProvider.Resend).Should().Be(1);
    }
}

using AuthForge.Application.EndUsers.Commands.VerifyEmail;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class VerifyEmailCommandValidatorTests
{
    private readonly VerifyEmailCommandValidator _validator;

    public VerifyEmailCommandValidatorTests()
    {
        _validator = new VerifyEmailCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new VerifyEmailCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validVerificationToken123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        Assert.Throws<ArgumentException>(() => Email.Create(""));
        Assert.Throws<ArgumentException>(() => Email.Create("   "));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyVerificationToken_ShouldFail(string verificationToken)
    {
        var command = new VerifyEmailCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            verificationToken);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(VerifyEmailCommand.VerificationToken));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("123456789")]
    public void Validate_WithVerificationTokenTooShort_ShouldFail(string shortToken)
    {
        var command = new VerifyEmailCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            shortToken);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(VerifyEmailCommand.VerificationToken));
    }
}

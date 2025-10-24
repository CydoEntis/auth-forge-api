using AuthForge.Application.EndUsers.Commands.ResetPassword;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator;

    public ResetPasswordCommandValidatorTests()
    {
        _validator = new ResetPasswordCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            "NewPassword123!",
            "NewPassword123!");

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
    public void Validate_WithEmptyResetToken_ShouldFail(string resetToken)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            resetToken,
            "NewPassword123!",
            "NewPassword123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.ResetToken));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("123456789")]
    public void Validate_WithResetTokenTooShort_ShouldFail(string shortToken)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            shortToken,
            "NewPassword123!",
            "NewPassword123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.ResetToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyNewPassword_ShouldFail(string newPassword)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            newPassword,
            newPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_WithNewPasswordTooShort_ShouldFail(string shortPassword)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            shortPassword,
            shortPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("nouppercase123!")]
    [InlineData("alllowercase")]
    public void Validate_WithNewPasswordMissingUppercase_ShouldFail(string password)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            password,
            password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("NOLOWERCASE123!")]
    [InlineData("ALLUPPERCASE")]
    public void Validate_WithNewPasswordMissingLowercase_ShouldFail(string password)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            password,
            password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("NoNumbers!")]
    [InlineData("OnlyLetters")]
    public void Validate_WithNewPasswordMissingNumber_ShouldFail(string password)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            password,
            password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyConfirmPassword_ShouldFail(string confirmPassword)
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            "NewPassword123!",
            confirmPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.ConfirmPassword));
    }

    [Fact]
    public void Validate_WithMismatchedPasswords_ShouldFail()
    {
        var command = new ResetPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            "validResetToken123",
            "NewPassword123!",
            "DifferentPassword123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.ConfirmPassword));
    }
}

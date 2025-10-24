using AuthForge.Application.EndUsers.Commands.ChangePassword;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _validator = new ChangePasswordCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            "NewPassword123!",
            "NewPassword123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyCurrentPassword_ShouldFail(string currentPassword)
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            currentPassword,
            "NewPassword123!",
            "NewPassword123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.CurrentPassword));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyNewPassword_ShouldFail(string newPassword)
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            newPassword,
            newPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_WithNewPasswordTooShort_ShouldFail(string shortPassword)
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            shortPassword,
            shortPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("nouppercase123!")]
    [InlineData("alllowercase")]
    public void Validate_WithNewPasswordMissingUppercase_ShouldFail(string password)
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            password,
            password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("NOLOWERCASE123!")]
    [InlineData("ALLUPPERCASE")]
    public void Validate_WithNewPasswordMissingLowercase_ShouldFail(string password)
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            password,
            password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("NoNumbers!")]
    [InlineData("OnlyLetters")]
    public void Validate_WithNewPasswordMissingNumber_ShouldFail(string password)
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            password,
            password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WithSameCurrentAndNewPassword_ShouldFail()
    {
        var samePassword = "SamePassword123!";
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            samePassword,
            samePassword,
            samePassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyConfirmPassword_ShouldFail(string confirmPassword)
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            "NewPassword123!",
            confirmPassword);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.ConfirmPassword));
    }

    [Fact]
    public void Validate_WithMismatchedPasswords_ShouldFail()
    {
        var command = new ChangePasswordCommand(
            EndUserId.CreateUnique(),
            "CurrentPassword123!",
            "NewPassword123!",
            "DifferentPassword123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordCommand.ConfirmPassword));
    }
}

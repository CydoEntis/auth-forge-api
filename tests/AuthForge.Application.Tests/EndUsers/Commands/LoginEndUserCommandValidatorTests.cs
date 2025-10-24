using AuthForge.Application.EndUsers.Commands.Login;
using FluentAssertions;
using Xunit;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class LoginEndUserCommandValidatorTests
{
    private readonly LoginEndUserCommandValidator _validator;

    public LoginEndUserCommandValidatorTests()
    {
        _validator = new LoginEndUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new LoginEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyApplicationId_ShouldFail(string applicationId)
    {
        var command = new LoginEndUserCommand(
            applicationId,
            "user@example.com",
            "Password123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginEndUserCommand.ApplicationId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_ShouldFail(string email)
    {
        var command = new LoginEndUserCommand(
            Guid.NewGuid().ToString(),
            email,
            "Password123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginEndUserCommand.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string invalidEmail)
    {
        var command = new LoginEndUserCommand(
            Guid.NewGuid().ToString(),
            invalidEmail,
            "Password123!");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginEndUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyPassword_ShouldFail(string password)
    {
        var command = new LoginEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            password);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginEndUserCommand.Password));
    }
}

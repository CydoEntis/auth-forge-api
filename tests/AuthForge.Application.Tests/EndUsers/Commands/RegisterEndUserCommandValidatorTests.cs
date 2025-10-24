using AuthForge.Application.EndUsers.Commands.Register;
using FluentAssertions;
using Xunit;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class RegisterEndUserCommandValidatorTests
{
    private readonly RegisterEndUserCommandValidator _validator;

    public RegisterEndUserCommandValidatorTests()
    {
        _validator = new RegisterEndUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyApplicationId_ShouldFail(string applicationId)
    {
        var command = new RegisterEndUserCommand(
            applicationId,
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.ApplicationId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_ShouldFail(string email)
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            email,
            "Password123!",
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string invalidEmail)
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            invalidEmail,
            "Password123!",
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.Email));
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldFail()
    {
        var longEmail = new string('a', 250) + "@example.com";
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            longEmail,
            "Password123!",
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyPassword_ShouldFail(string password)
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            password,
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.Password));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_WithPasswordTooShort_ShouldFail(string shortPassword)
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            shortPassword,
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.Password));
    }

    [Fact]
    public void Validate_WithPasswordTooLong_ShouldFail()
    {
        var longPassword = new string('a', 101);
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            longPassword,
            "John",
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyFirstName_ShouldFail(string firstName)
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!",
            firstName,
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.FirstName));
    }

    [Fact]
    public void Validate_WithFirstNameTooLong_ShouldFail()
    {
        var longFirstName = new string('a', 101);
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!",
            longFirstName,
            "Doe");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyLastName_ShouldFail(string lastName)
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!",
            "John",
            lastName);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.LastName));
    }

    [Fact]
    public void Validate_WithLastNameTooLong_ShouldFail()
    {
        var longLastName = new string('a', 101);
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!",
            "John",
            longLastName);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterEndUserCommand.LastName));
    }
}

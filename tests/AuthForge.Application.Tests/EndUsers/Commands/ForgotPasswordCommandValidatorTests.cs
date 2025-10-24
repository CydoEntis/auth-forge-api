using AuthForge.Application.EndUsers.Commands.ForgotPassword;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class ForgotPasswordCommandValidatorTests
{
    private readonly ForgotPasswordCommandValidator _validator;

    public ForgotPasswordCommandValidatorTests()
    {
        _validator = new ForgotPasswordCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new ForgotPasswordCommand(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"));

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
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string invalidEmail)
    {
        Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
    }
}

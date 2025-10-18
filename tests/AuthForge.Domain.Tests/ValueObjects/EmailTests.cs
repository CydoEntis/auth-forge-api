using AuthForge.Domain.ValueObjects;
using FluentAssertions;

namespace AuthForge.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        string validEmail = "test@test.com";

        var email = Email.Create(validEmail);

        email.Value.Should().Be("test@test.com");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldFail()
    {
        string invalidEmai = "invalid-email";

        Action act = () => Email.Create(invalidEmai);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    [InlineData("@no-local-part.com")]
    public void Create_WithInvalidEmails_ShouldThrowException(string invalidEmail)
    {
        Action act = () => Email.Create(invalidEmail);
        act.Should().Throw<ArgumentException>();
    }
    
}
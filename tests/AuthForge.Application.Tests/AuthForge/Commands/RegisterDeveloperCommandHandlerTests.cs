using AuthForge.Application.AuthForge.Commands.Register;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.AuthForge.Commands;

public class RegisterDeveloperCommandHandlerTests
{
    private readonly Mock<IAuthForgeUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailParser> _emailParserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterDeveloperCommandHandler _handler;

    public RegisterDeveloperCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IAuthForgeUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailParserMock = new Mock<IEmailParser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterDeveloperCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _emailParserMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldRegisterDeveloper()
    {
        var command = new RegisterDeveloperCommand(
            "test@example.com",
            "Password123!",
            "John",
            "Doe");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("test@example.com")));

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns(HashedPassword.FromHash("hashed", "salt"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FullName.Should().Be("John Doe");

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuthForgeUser>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnDuplicateError()
    {
        var command = new RegisterDeveloperCommand(
            "existing@example.com",
            "Password123!",
            "John",
            "Doe");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("existing@example.com")));

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeUserErrors.DuplicateEmail);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuthForgeUser>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithInvalidEmail_ShouldReturnValidationError(string invalidEmail)
    {
        var command = new RegisterDeveloperCommand(
            invalidEmail,
            "Password123!",
            "John",
            "Doe");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Failure(ValidationErrors.InvalidEmail()));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        var command = new RegisterDeveloperCommand(
            "test@example.com",
            "PlainPassword123!",
            "John",
            "Doe");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("test@example.com")));

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword("PlainPassword123!"))
            .Returns(HashedPassword.FromHash("hashed", "salt"));

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasherMock.Verify(
            x => x.HashPassword("PlainPassword123!"),
            Times.Once);
    }
}
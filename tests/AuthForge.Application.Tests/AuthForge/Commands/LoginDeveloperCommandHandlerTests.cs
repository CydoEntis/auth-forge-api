using AuthForge.Application.AuthForge.Commands.Login;
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

public class LoginDeveloperCommandHandlerTests
{
    private readonly Mock<IAuthForgeUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthForgeRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IAuthForgeJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailParser> _emailParserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginDeveloperCommandHandler _handler;

    public LoginDeveloperCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IAuthForgeUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IAuthForgeRefreshTokenRepository>();
        _tokenGeneratorMock = new Mock<IAuthForgeJwtTokenGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailParserMock = new Mock<IEmailParser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LoginDeveloperCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenGeneratorMock.Object,
            _passwordHasherMock.Object,
            _emailParserMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hashed", "salt"),
            "John",
            "Doe");
        var command = new LoginDeveloperCommand("test@example.com", "Password123!");

        var tokenPair = new TokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("test@example.com")));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<HashedPassword>()))
            .Returns(true);

        _tokenGeneratorMock
            .Setup(x => x.GenerateTokenPair(It.IsAny<AuthForgeUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(tokenPair);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuthForgeRefreshToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FullName.Should().Be("John Doe");

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuthForgeRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnInvalidCredentialsError()
    {
        var command = new LoginDeveloperCommand("nonexistent@example.com", "Password123!");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("nonexistent@example.com")));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthForgeUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeUserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnInvalidCredentialsError()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hashed", "salt"),
            "John",
            "Doe");
        var command = new LoginDeveloperCommand("test@example.com", "WrongPassword!");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("test@example.com")));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<HashedPassword>()))
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeUserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnInactiveError()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hashed", "salt"),
            "John",
            "Doe");
        user.Deactivate();
        var command = new LoginDeveloperCommand("test@example.com", "Password123!");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("test@example.com")));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<HashedPassword>()))
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeUserErrors.Inactive);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithInvalidEmailFormat_ShouldReturnValidationError(string invalidEmail)
    {
        var command = new LoginDeveloperCommand(invalidEmail, "Password123!");

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Failure(ValidationErrors.InvalidEmail()));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
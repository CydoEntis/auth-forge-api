using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Application.EndUsers.Commands.Login;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class LoginEndUserCommandHandlerTests
{
    private readonly Mock<IEndUserRepository> _endUserRepositoryMock;
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IEndUserRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IEndUserJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailParser> _emailParserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<LoginEndUserCommandHandler>> _loggerMock;
    private readonly LoginEndUserCommandHandler _handler;

    public LoginEndUserCommandHandlerTests()
    {
        _endUserRepositoryMock = new Mock<IEndUserRepository>();
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _refreshTokenRepositoryMock = new Mock<IEndUserRefreshTokenRepository>();
        _tokenGeneratorMock = new Mock<IEndUserJwtTokenGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailParserMock = new Mock<IEmailParser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<LoginEndUserCommandHandler>>();

        _handler = new LoginEndUserCommandHandler(
            _endUserRepositoryMock.Object,
            _applicationRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenGeneratorMock.Object,
            _passwordHasherMock.Object,
            _emailParserMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            "Test App",
            "test-app",
            null,
            null);

        var endUser = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        var command = new LoginEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "Password123!");

        var tokenPair = new TokenPair(
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("user@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<HashedPassword>()))
            .Returns(true);

        _tokenGeneratorMock
            .Setup(x => x.GenerateTokenPair(It.IsAny<EndUser>(), It.IsAny<Domain.Entities.Application>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .Returns(tokenPair);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EndUserRefreshToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.Email.Should().Be("user@example.com");
        result.Value.FullName.Should().Be("Jane Smith");

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<EndUserRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WithInvalidApplicationId_ShouldReturnValidationError()
    {
        var command = new LoginEndUserCommand(
            "not-a-guid",
            "user@example.com",
            "Password123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnNotFoundError()
    {
        var command = new LoginEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Application?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInactiveApplication_ShouldReturnInactiveError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            "Test App",
            "test-app",
            null,
            null);
        application.Deactivate();

        var command = new LoginEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "Password123!");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Inactive);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnInvalidCredentialsError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            "Test App",
            "test-app",
            null,
            null);

        var command = new LoginEndUserCommand(
            applicationId.Value.ToString(),
            "nonexistent@example.com",
            "Password123!");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("nonexistent@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnInvalidCredentialsError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            "Test App",
            "test-app",
            null,
            null);

        var endUser = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        var command = new LoginEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "WrongPassword!");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("user@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<HashedPassword>()))
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnInactiveError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            "Test App",
            "test-app",
            null,
            null);

        var endUser = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        endUser.Deactivate();

        var command = new LoginEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "Password123!");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("user@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<HashedPassword>()))
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.Inactive);
    }

    [Fact]
    public async Task Handle_WithLockedOutUser_ShouldReturnLockedOutError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            "Test App",
            "test-app",
            null,
            null);

        var endUser = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        for (int i = 0; i < 5; i++)
        {
            endUser.RecordFailedLogin(5, 15);
        }

        var command = new LoginEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "Password123!");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("user@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<HashedPassword>()))
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EndUser.LockedOut");
    }
}
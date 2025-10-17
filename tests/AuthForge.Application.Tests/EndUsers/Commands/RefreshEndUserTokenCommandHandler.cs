using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.Refresh;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class RefreshEndUserTokenCommandHandlerTests
{
    private readonly Mock<IEndUserRepository> _endUserRepositoryMock;
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IEndUserRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IEndUserJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RefreshEndUserTokenCommandHandler _handler;

    public RefreshEndUserTokenCommandHandlerTests()
    {
        _endUserRepositoryMock = new Mock<IEndUserRepository>();
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _refreshTokenRepositoryMock = new Mock<IEndUserRefreshTokenRepository>();
        _tokenGeneratorMock = new Mock<IEndUserJwtTokenGenerator>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RefreshEndUserTokenCommandHandler(
            _endUserRepositoryMock.Object,
            _applicationRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenGeneratorMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            AuthForgeUserId.Create(Guid.NewGuid()),
            "Test App",
            "test-app");

        var endUserId = EndUserId.Create(Guid.NewGuid());
        var endUser = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        var refreshToken = EndUserRefreshToken.Create(
            endUserId,
            "valid-refresh-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Mozilla");

        var command = new RefreshEndUserTokenCommand("valid-refresh-token");

        var newTokenPair = new TokenPair(
            "new-access-token",
            "new-refresh-token",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _tokenGeneratorMock
            .Setup(x => x.GenerateTokenPair(It.IsAny<EndUser>(), It.IsAny<Domain.Entities.Application>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(newTokenPair);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EndUserRefreshToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<EndUserRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnInvalidTokenError()
    {
        var command = new RefreshEndUserTokenCommand("invalid-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUserRefreshToken?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserRefreshTokenErrors.Invalid);
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldReturnExpiredError()
    {
        var endUserId = EndUserId.Create(Guid.NewGuid());
        // Create token with very short expiration, then wait for it to expire
        var refreshToken = EndUserRefreshToken.Create(
            endUserId,
            "expired-token",
            DateTime.UtcNow.AddMilliseconds(1),
            "127.0.0.1",
            "Mozilla");

        await Task.Delay(10); // Wait for token to expire

        var command = new RefreshEndUserTokenCommand("expired-token");

        var endUser = EndUser.Create(
            Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Domain.Entities.Application.Create(
                AuthForgeUserId.Create(Guid.NewGuid()),
                "Test App",
                "test-app"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserRefreshTokenErrors.Expired);
    }

    [Fact]
    public async Task Handle_WithRevokedRefreshToken_ShouldReturnRevokedError()
    {
        var endUserId = EndUserId.Create(Guid.NewGuid());
        var refreshToken = EndUserRefreshToken.Create(
            endUserId,
            "revoked-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Mozilla");
        refreshToken.Revoke();

        var command = new RefreshEndUserTokenCommand("revoked-token");

        var endUser = EndUser.Create(
            Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid()),
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Domain.Entities.Application.Create(
                AuthForgeUserId.Create(Guid.NewGuid()),
                "Test App",
                "test-app"));

        _refreshTokenRepositoryMock
            .Setup(x => x.GetActiveTokensForUserAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EndUserRefreshToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserRefreshTokenErrors.Revoked);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnInactiveError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var endUserId = EndUserId.Create(Guid.NewGuid());
        
        var endUser = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");
        endUser.Deactivate();

        var refreshToken = EndUserRefreshToken.Create(
            endUserId,
            "valid-refresh-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Mozilla");

        var command = new RefreshEndUserTokenCommand("valid-refresh-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Domain.Entities.Application.Create(
                AuthForgeUserId.Create(Guid.NewGuid()),
                "Test App",
                "test-app"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.Inactive);
    }

    [Fact]
    public async Task Handle_WithInactiveApplication_ShouldReturnInactiveError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            AuthForgeUserId.Create(Guid.NewGuid()),
            "Test App",
            "test-app");
        application.Deactivate();

        var endUserId = EndUserId.Create(Guid.NewGuid());
        var endUser = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        var refreshToken = EndUserRefreshToken.Create(
            endUserId,
            "valid-refresh-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Mozilla");

        var command = new RefreshEndUserTokenCommand("valid-refresh-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<EndUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Inactive);
    }
}
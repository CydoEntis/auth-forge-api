using AuthForge.Application.AuthForge.Commands.Refresh;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.AuthForge.Commands;

public class RefreshDeveloperTokenCommandHandlerTests
{
    private readonly Mock<IAuthForgeUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthForgeRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IAuthForgeJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RefreshDeveloperTokenCommandHandler _handler;

    public RefreshDeveloperTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IAuthForgeUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IAuthForgeRefreshTokenRepository>();
        _tokenGeneratorMock = new Mock<IAuthForgeJwtTokenGenerator>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RefreshDeveloperTokenCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenGeneratorMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        var refreshToken = AuthForgeRefreshToken.Create(
            userId,
            "valid-refresh-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Mozilla");

        var command = new RefreshDeveloperTokenCommand("valid-refresh-token");

        var newTokenPair = new TokenPair(
            "new-access-token",
            "new-refresh-token",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenGeneratorMock
            .Setup(x => x.GenerateTokenPair(It.IsAny<AuthForgeUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(newTokenPair);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuthForgeRefreshToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuthForgeRefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnInvalidTokenError()
    {
        var command = new RefreshDeveloperTokenCommand("invalid-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthForgeRefreshToken?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeRefreshTokenErrors.Invalid);
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldReturnExpiredError()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        // Create token with very short expiration, then wait for it to expire
        var refreshToken = AuthForgeRefreshToken.Create(
            userId,
            "expired-token",
            DateTime.UtcNow.AddMilliseconds(1),
            "127.0.0.1",
            "Mozilla");

        await Task.Delay(10); // Wait for token to expire

        var command = new RefreshDeveloperTokenCommand("expired-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthForgeUser.Create(
                Email.Create("test@example.com"),
                HashedPassword.FromHash("hash", "salt"),
                "John",
                "Doe"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeRefreshTokenErrors.Expired);
    }

    [Fact]
    public async Task Handle_WithRevokedRefreshToken_ShouldReturnRevokedError()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var refreshToken = AuthForgeRefreshToken.Create(
            userId,
            "revoked-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Mozilla");
        refreshToken.Revoke();

        var command = new RefreshDeveloperTokenCommand("revoked-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthForgeUser.Create(
                Email.Create("test@example.com"),
                HashedPassword.FromHash("hash", "salt"),
                "John",
                "Doe"));

        _refreshTokenRepositoryMock
            .Setup(x => x.GetActiveTokensForUserAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuthForgeRefreshToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeRefreshTokenErrors.Revoked);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnInactiveError()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        user.Deactivate();

        var refreshToken = AuthForgeRefreshToken.Create(
            userId,
            "valid-refresh-token",
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1",
            "Mozilla");

        var command = new RefreshDeveloperTokenCommand("valid-refresh-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeUserErrors.Inactive);
    }
}
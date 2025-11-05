using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.Refresh;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class RefreshEndUserTokenCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IEndUserRefreshTokenRepository _refreshTokenRepository;
    private readonly IEndUserJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshEndUserTokenCommandHandler> _logger;
    private readonly RefreshEndUserTokenCommandHandler _handler;

    public RefreshEndUserTokenCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _refreshTokenRepository = Substitute.For<IEndUserRefreshTokenRepository>();
        _jwtTokenGenerator = Substitute.For<IEndUserJwtTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<RefreshEndUserTokenCommandHandler>>();

        _handler = new RefreshEndUserTokenCommandHandler(
            _endUserRepository,
            _applicationRepository,
            _refreshTokenRepository,
            _jwtTokenGenerator,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokenPair()
    {
        var refreshTokenValue = "valid-refresh-token";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";
        var command = new RefreshEndUserTokenCommand(refreshTokenValue, ipAddress, userAgent);

        var applicationId = ApplicationId.CreateUnique();
        var user = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var refreshToken = EndUserRefreshToken.Create(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            ipAddress,
            userAgent);

        var newTokenPair = new TokenPair(
            "new-access-token",
            "new-refresh-token",
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));

        _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _endUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _jwtTokenGenerator.GenerateTokenPair(user, application, ipAddress, userAgent)
            .Returns(newTokenPair);
        _refreshTokenRepository.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(new List<EndUserRefreshToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<EndUserRefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailure()
    {
        var command = new RefreshEndUserTokenCommand("invalid-token", "192.168.1.1", "Mozilla/5.0");

        _refreshTokenRepository.GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((EndUserRefreshToken?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserRefreshTokenErrors.Invalid);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var refreshTokenValue = "valid-refresh-token";
        var command = new RefreshEndUserTokenCommand(refreshTokenValue, "192.168.1.1", "Mozilla/5.0");

        var userId = EndUserId.CreateUnique();
        var refreshToken = EndUserRefreshToken.Create(
            userId,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Mozilla/5.0");

        _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnFailure()
    {
        var refreshTokenValue = "valid-refresh-token";
        var command = new RefreshEndUserTokenCommand(refreshTokenValue, "192.168.1.1", "Mozilla/5.0");

        var userId = EndUserId.CreateUnique();
        var applicationId = ApplicationId.CreateUnique();
        var user = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var refreshToken = EndUserRefreshToken.Create(
            userId,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Mozilla/5.0");

        _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns((AuthForge.Domain.Entities.Application?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldReturnFailure()
    {
        var refreshTokenValue = "expired-refresh-token";
        var command = new RefreshEndUserTokenCommand(refreshTokenValue, "192.168.1.1", "Mozilla/5.0");

        var applicationId = ApplicationId.CreateUnique();
        var user = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);

        var refreshToken = EndUserRefreshToken.Create(
            user.Id,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(1),
            "192.168.1.1",
            "Mozilla/5.0");

        var expiresProperty = typeof(EndUserRefreshToken).GetProperty("ExpiresAtUtc");
        expiresProperty!.SetValue(refreshToken, DateTime.UtcNow.AddDays(-1));

        _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _endUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserRefreshTokenErrors.Expired);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        var refreshTokenValue = "valid-refresh-token";
        var command = new RefreshEndUserTokenCommand(refreshTokenValue, "192.168.1.1", "Mozilla/5.0");

        var userId = EndUserId.CreateUnique();
        var applicationId = ApplicationId.CreateUnique();
        var user = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.Deactivate(); 

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var refreshToken = EndUserRefreshToken.Create(
            userId,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Mozilla/5.0");

        _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.Inactive);
    }

    [Fact]
    public async Task Handle_WithInactiveApplication_ShouldReturnFailure()
    {
        var refreshTokenValue = "valid-refresh-token";
        var command = new RefreshEndUserTokenCommand(refreshTokenValue, "192.168.1.1", "Mozilla/5.0");

        var userId = EndUserId.CreateUnique();
        var applicationId = ApplicationId.CreateUnique();
        var user = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        application.Deactivate(); 

        var refreshToken = EndUserRefreshToken.Create(
            userId,
            refreshTokenValue,
            DateTime.UtcNow.AddDays(7),
            "192.168.1.1",
            "Mozilla/5.0");

        _refreshTokenRepository.GetByTokenAsync(refreshTokenValue, Arg.Any<CancellationToken>())
            .Returns(refreshToken);
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Inactive);
    }
}

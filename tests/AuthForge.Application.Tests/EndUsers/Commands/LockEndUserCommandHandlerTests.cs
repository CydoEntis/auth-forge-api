using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.LockEndUser;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class LockEndUserCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LockEndUserCommandHandler> _logger;
    private readonly LockEndUserCommandHandler _handler;

    public LockEndUserCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<LockEndUserCommandHandler>>();

        _handler = new LockEndUserCommandHandler(
            _endUserRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithUnlockedUser_ShouldLockUser()
    {
        var userId = EndUserId.CreateUnique();
        var lockoutMinutes = 30;
        var command = new LockEndUserCommand(userId, lockoutMinutes);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var beforeLock = DateTime.UtcNow;
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        var afterLock = DateTime.UtcNow;
        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("locked successfully");
        result.Value.LockedOutUntil.Should().BeAfter(beforeLock.AddMinutes(lockoutMinutes - 1));
        result.Value.LockedOutUntil.Should().BeBefore(afterLock.AddMinutes(lockoutMinutes + 1));
        user.LockedOutUntil.Should().NotBeNull();
        user.IsLockedOut().Should().BeTrue();
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new LockEndUserCommand(userId, 30);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithAlreadyLockedUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new LockEndUserCommand(userId, 30);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.ManualLock(60); 

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.AlreadyLockedOut);
    }

    [Fact]
    public async Task Handle_ShouldSetLockoutDuration()
    {
        var userId = EndUserId.CreateUnique();
        var lockoutMinutes = 45;
        var command = new LockEndUserCommand(userId, lockoutMinutes);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var beforeLock = DateTime.UtcNow;
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        var afterLock = DateTime.UtcNow;
        user.LockedOutUntil.Should().NotBeNull();
        user.LockedOutUntil.Should().BeAfter(beforeLock.AddMinutes(lockoutMinutes - 1));
        user.LockedOutUntil.Should().BeBefore(afterLock.AddMinutes(lockoutMinutes + 1));
    }
}

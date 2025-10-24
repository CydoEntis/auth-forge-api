using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.UnlockEndUser;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class UnlockEndUserCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UnlockEndUserCommandHandler _handler;

    public UnlockEndUserCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new UnlockEndUserCommandHandler(
            _endUserRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithLockedUser_ShouldUnlockUser()
    {
        var userId = EndUserId.CreateUnique();
        var command = new UnlockEndUserCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.ManualLock(30); 

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("unlocked successfully");
        user.LockedOutUntil.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLockedOut().Should().BeFalse();
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new UnlockEndUserCommand(userId);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithNotLockedUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new UnlockEndUserCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotLockedOut);
    }

    [Fact]
    public async Task Handle_ShouldResetFailedLoginAttempts()
    {
        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        // Record 5 failed login attempts to lock the user
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin(5, 30);
        }

        var command = new UnlockEndUserCommand(user.Id);

        _endUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        user.FailedLoginAttempts.Should().Be(0);
    }
}

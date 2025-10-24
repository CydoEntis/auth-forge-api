using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.DeactivateEndUser;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class DeactivateEndUserCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeactivateEndUserCommandHandler _handler;

    public DeactivateEndUserCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new DeactivateEndUserCommandHandler(
            _endUserRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithActiveUser_ShouldDeactivateUser()
    {
        var userId = EndUserId.CreateUnique();
        var command = new DeactivateEndUserCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("deactivated successfully");
        user.IsActive.Should().BeFalse();
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new DeactivateEndUserCommand(userId);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithAlreadyDeactivatedUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new DeactivateEndUserCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.Deactivate(); 

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.AlreadyDeactivated);
    }
}

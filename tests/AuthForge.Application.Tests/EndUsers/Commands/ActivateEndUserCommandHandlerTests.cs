using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.ActivateEndUser;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class ActivateEndUserCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateEndUserCommandHandler> _logger;
    private readonly ActivateEndUserCommandHandler _handler;

    public ActivateEndUserCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<ActivateEndUserCommandHandler>>();

        _handler = new ActivateEndUserCommandHandler(
            _endUserRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithDeactivatedUser_ShouldActivateUser()
    {
        var userId = EndUserId.CreateUnique();
        var command = new ActivateEndUserCommand(userId);

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

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("activated successfully");
        user.IsActive.Should().BeTrue();
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new ActivateEndUserCommand(userId);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithAlreadyActiveUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new ActivateEndUserCommand(userId);

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
        result.Error.Should().Be(EndUserErrors.AlreadyActive);
    }
}

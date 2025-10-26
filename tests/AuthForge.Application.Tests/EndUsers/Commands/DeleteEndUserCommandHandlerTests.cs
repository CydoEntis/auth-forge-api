using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.DeleteEndUser;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class DeleteEndUserCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteEndUserCommandHandler> _logger;
    private readonly DeleteEndUserCommandHandler _handler;

    public DeleteEndUserCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<DeleteEndUserCommandHandler>>();

        _handler = new DeleteEndUserCommandHandler(
            _endUserRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldDeleteUser()
    {
        var userId = EndUserId.CreateUnique();
        var command = new DeleteEndUserCommand(userId);

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
        result.Value.Message.Should().Contain("deleted successfully");
        _endUserRepository.Received(1).Delete(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new DeleteEndUserCommand(userId);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
        _endUserRepository.DidNotReceive().Delete(Arg.Any<EndUser>());
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteOnRepository()
    {
        var userId = EndUserId.CreateUnique();
        var command = new DeleteEndUserCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        _endUserRepository.Received(1).Delete(user);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var userId = EndUserId.CreateUnique();
        var command = new DeleteEndUserCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

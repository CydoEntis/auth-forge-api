using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.UpdateCurrentUser;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class UpdateCurrentUserCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCurrentUserCommandHandler _handler;

    public UpdateCurrentUserCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new UpdateCurrentUserCommandHandler(
            _endUserRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateProfile()
    {
        var userId = EndUserId.CreateUnique();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var command = new UpdateCurrentUserCommand(userId, newFirstName, newLastName);

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
        result.Value.Message.Should().Contain("updated successfully");
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new UpdateCurrentUserCommand(userId, "Jane", "Smith");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithEmptyFirstName_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new UpdateCurrentUserCommand(userId, "", "Smith");

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
        result.Error.Code.Should().Be("EndUser.InvalidInput");
    }

    [Fact]
    public async Task Handle_WithEmptyLastName_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new UpdateCurrentUserCommand(userId, "Jane", "");

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
        result.Error.Code.Should().Be("EndUser.InvalidInput");
    }

    [Fact]
    public async Task Handle_ShouldUpdateBothNames()
    {
        var userId = EndUserId.CreateUnique();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var command = new UpdateCurrentUserCommand(userId, newFirstName, newLastName);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var originalFirstName = user.FirstName;
        var originalLastName = user.LastName;

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        user.FirstName.Should().NotBe(originalFirstName);
        user.LastName.Should().NotBe(originalLastName);
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
    }
}

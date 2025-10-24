using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.ChangePassword;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new ChangePasswordCommandHandler(
            _endUserRepository,
            _passwordHasher,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldChangePassword()
    {
        var userId = EndUserId.CreateUnique();
        var currentPassword = "OldPassword123!";
        var currentHashedPassword = HashedPassword.Create(currentPassword);
        var newPassword = "NewPassword123!";
        var command = new ChangePasswordCommand(
            userId,
            currentPassword,
            newPassword,
            newPassword);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            currentHashedPassword,
            "John",
            "Doe");

        var newHashedPassword = HashedPassword.Create(newPassword);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(currentPassword, currentHashedPassword)
            .Returns(true);
        _passwordHasher.HashPassword(newPassword)
            .Returns(newHashedPassword);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("changed successfully");
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new ChangePasswordCommand(
            userId,
            "OldPassword123!",
            "NewPassword123!",
            "NewPassword123!");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithIncorrectCurrentPassword_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var correctPassword = "CorrectPassword123!";
        var incorrectPassword = "WrongPassword123!";
        var command = new ChangePasswordCommand(
            userId,
            incorrectPassword,
            "NewPassword123!",
            "NewPassword123!");

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create(correctPassword),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(incorrectPassword, user.PasswordHash)
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_ShouldVerifyCurrentPasswordBeforeChanging()
    {
        var currentPassword = "OldPassword123!";
        var currentHashedPassword = HashedPassword.Create(currentPassword);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            currentHashedPassword,
            "John",
            "Doe");

        var command = new ChangePasswordCommand(
            user.Id,
            currentPassword,
            "NewPassword123!",
            "NewPassword123!");

        _endUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(currentPassword, currentHashedPassword)
            .Returns(true);
        _passwordHasher.HashPassword(Arg.Any<string>())
            .Returns(HashedPassword.Create("NewPassword123!"));

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).VerifyPassword(currentPassword, currentHashedPassword);
    }

    [Fact]
    public async Task Handle_ShouldHashNewPassword()
    {
        var userId = EndUserId.CreateUnique();
        var currentPassword = "OldPassword123!";
        var currentHashedPassword = HashedPassword.Create(currentPassword);
        var newPassword = "NewPassword123!";
        var command = new ChangePasswordCommand(
            userId,
            currentPassword,
            newPassword,
            newPassword);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            currentHashedPassword,
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(currentPassword, currentHashedPassword)
            .Returns(true);
        _passwordHasher.HashPassword(newPassword)
            .Returns(HashedPassword.Create(newPassword));

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword(newPassword);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserInRepository()
    {
        var currentPassword = "OldPassword123!";
        var currentHashedPassword = HashedPassword.Create(currentPassword);
        var newPassword = "NewPassword123!";

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            currentHashedPassword,
            "John",
            "Doe");

        var command = new ChangePasswordCommand(
            user.Id,
            currentPassword,
            newPassword,
            newPassword);

        _endUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(currentPassword, currentHashedPassword)
            .Returns(true);
        _passwordHasher.HashPassword(newPassword)
            .Returns(HashedPassword.Create(newPassword));

        await _handler.Handle(command, CancellationToken.None);

        _endUserRepository.Received(1).Update(user);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var userId = EndUserId.CreateUnique();
        var currentPassword = "OldPassword123!";
        var currentHashedPassword = HashedPassword.Create(currentPassword);
        var newPassword = "NewPassword123!";
        var command = new ChangePasswordCommand(
            userId,
            currentPassword,
            newPassword,
            newPassword);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            currentHashedPassword,
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(currentPassword, currentHashedPassword)
            .Returns(true);
        _passwordHasher.HashPassword(newPassword)
            .Returns(HashedPassword.Create(newPassword));

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.ResetPassword;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class ResetPasswordCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<ResetPasswordCommandHandler>>();

        _handler = new ResetPasswordCommandHandler(
            _endUserRepository,
            _passwordHasher,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidTokenAndPassword_ShouldResetPassword()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var resetToken = "valid-reset-token-12345678";
        var newPassword = "NewPassword123!";
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            resetToken,
            newPassword,
            newPassword);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("OldPassword123!"),
            "John",
            "Doe");
        user.SetPasswordResetToken(resetToken, DateTime.UtcNow.AddHours(1));

        var newHashedPassword = HashedPassword.Create(newPassword);

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.HashPassword(newPassword)
            .Returns(newHashedPassword);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("reset successfully");
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("nonexistent@example.com");
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            "reset-token",
            "NewPassword123!",
            "NewPassword123!");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInvalidResetToken_ShouldReturnFailure()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var validToken = "valid-token-12345678";
        var invalidToken = "invalid-token-87654321";
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            invalidToken,
            "NewPassword123!",
            "NewPassword123!");

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("OldPassword123!"),
            "John",
            "Doe");
        user.SetPasswordResetToken(validToken, DateTime.UtcNow.AddHours(1));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.InvalidResetToken);
    }

    [Fact]
    public async Task Handle_WithExpiredResetToken_ShouldReturnFailure()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var resetToken = "expired-token-12345678";
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            resetToken,
            "NewPassword123!",
            "NewPassword123!");

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("OldPassword123!"),
            "John",
            "Doe");

        var tokenProperty = typeof(EndUser).GetProperty("PasswordResetToken");
        var expiresProperty = typeof(EndUser).GetProperty("PasswordResetTokenExpiresAt");
        tokenProperty!.SetValue(user, resetToken);
        expiresProperty!.SetValue(user, DateTime.UtcNow.AddHours(-1)); 

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.InvalidResetToken);
    }

    [Fact]
    public async Task Handle_ShouldHashNewPassword()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var resetToken = "valid-token-12345678";
        var newPassword = "NewPassword123!";
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            resetToken,
            newPassword,
            newPassword);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("OldPassword123!"),
            "John",
            "Doe");
        user.SetPasswordResetToken(resetToken, DateTime.UtcNow.AddHours(1));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.HashPassword(newPassword)
            .Returns(HashedPassword.Create(newPassword));

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword(newPassword);
    }

    [Fact]
    public async Task Handle_ShouldClearResetToken()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var resetToken = "valid-token-12345678";
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            resetToken,
            "NewPassword123!",
            "NewPassword123!");

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("OldPassword123!"),
            "John",
            "Doe");
        user.SetPasswordResetToken(resetToken, DateTime.UtcNow.AddHours(1));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.HashPassword(Arg.Any<string>())
            .Returns(HashedPassword.Create("NewPassword123!"));

        await _handler.Handle(command, CancellationToken.None);

        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserInRepository()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var resetToken = "valid-token-12345678";
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            resetToken,
            "NewPassword123!",
            "NewPassword123!");

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("OldPassword123!"),
            "John",
            "Doe");
        user.SetPasswordResetToken(resetToken, DateTime.UtcNow.AddHours(1));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.HashPassword(Arg.Any<string>())
            .Returns(HashedPassword.Create("NewPassword123!"));

        await _handler.Handle(command, CancellationToken.None);

        _endUserRepository.Received(1).Update(user);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var resetToken = "valid-token-12345678";
        var command = new ResetPasswordCommand(
            applicationId,
            email,
            resetToken,
            "NewPassword123!",
            "NewPassword123!");

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("OldPassword123!"),
            "John",
            "Doe");
        user.SetPasswordResetToken(resetToken, DateTime.UtcNow.AddHours(1));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.HashPassword(Arg.Any<string>())
            .Returns(HashedPassword.Create("NewPassword123!"));

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

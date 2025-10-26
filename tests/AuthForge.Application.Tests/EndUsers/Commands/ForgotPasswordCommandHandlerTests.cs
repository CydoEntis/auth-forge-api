using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.ForgotPassword;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<ForgotPasswordCommandHandler>>();

        _handler = new ForgotPasswordCommandHandler(
            _endUserRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldSetPasswordResetToken()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var command = new ForgotPasswordCommand(applicationId, email);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("password reset email has been sent");
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetTokenExpiresAt.Should().NotBeNull();
        user.PasswordResetTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldStillReturnSuccess()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("nonexistent@example.com");
        var command = new ForgotPasswordCommand(applicationId, email);

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("password reset email has been sent");
        _endUserRepository.DidNotReceive().Update(Arg.Any<EndUser>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateResetTokenThatIsNotEmpty()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var command = new ForgotPasswordCommand(applicationId, email);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        user.PasswordResetToken.Should().NotBeNullOrWhiteSpace();
        user.PasswordResetToken!.Length.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task Handle_ShouldSetExpirationTimeOneHourInFuture()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var command = new ForgotPasswordCommand(applicationId, email);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var beforeCall = DateTime.UtcNow;
        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        var afterCall = DateTime.UtcNow;
        user.PasswordResetTokenExpiresAt.Should().NotBeNull();
        user.PasswordResetTokenExpiresAt.Should().BeAfter(beforeCall.AddMinutes(59));
        user.PasswordResetTokenExpiresAt.Should().BeBefore(afterCall.AddMinutes(61));
    }

    [Fact]
    public async Task Handle_CalledMultipleTimes_ShouldGenerateDifferentTokens()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var command = new ForgotPasswordCommand(applicationId, email);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);
        var firstToken = user.PasswordResetToken;

        await _handler.Handle(command, CancellationToken.None);
        var secondToken = user.PasswordResetToken;

        firstToken.Should().NotBe(secondToken);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserInRepository()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var command = new ForgotPasswordCommand(applicationId, email);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        _endUserRepository.Received(1).Update(Arg.Is<EndUser>(u => u.Id == user.Id));
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var command = new ForgotPasswordCommand(applicationId, email);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

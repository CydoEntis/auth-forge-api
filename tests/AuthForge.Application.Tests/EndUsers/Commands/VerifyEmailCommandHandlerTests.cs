using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.VerifyEmail;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class VerifyEmailCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;
    private readonly VerifyEmailCommandHandler _handler;

    public VerifyEmailCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<VerifyEmailCommandHandler>>();

        _handler = new VerifyEmailCommandHandler(
            _endUserRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldVerifyEmail()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var verificationToken = "valid-verification-token";
        var command = new VerifyEmailCommand(applicationId, email, verificationToken);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.SetEmailVerificationToken(verificationToken, DateTime.UtcNow.AddHours(24));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Message.Should().Contain("verified successfully");
        user.IsEmailVerified.Should().BeTrue();
        user.EmailVerificationToken.Should().BeNull();
        user.EmailVerificationTokenExpiresAt.Should().BeNull();
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("nonexistent@example.com");
        var command = new VerifyEmailCommand(applicationId, email, "token");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithAlreadyVerifiedEmail_ShouldReturnFailure()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var command = new VerifyEmailCommand(applicationId, email, "token");

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.VerifyEmail(); 

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.EmailAlreadyVerified);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldReturnFailure()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var invalidToken = "invalid-token";
        var command = new VerifyEmailCommand(applicationId, email, invalidToken);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.InvalidVerificationToken);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnFailure()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var token = "expired-token";
        var command = new VerifyEmailCommand(applicationId, email, token);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var tokenProperty = typeof(EndUser).GetProperty(nameof(EndUser.EmailVerificationToken));
        var expiresProperty = typeof(EndUser).GetProperty(nameof(EndUser.EmailVerificationTokenExpiresAt));
        tokenProperty!.SetValue(user, token);
        expiresProperty!.SetValue(user, DateTime.UtcNow.AddHours(-1)); 

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.InvalidVerificationToken);
    }

    [Fact]
    public async Task Handle_ShouldClearVerificationTokenAfterSuccess()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var token = "valid-token";
        var command = new VerifyEmailCommand(applicationId, email, token);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.SetEmailVerificationToken(token, DateTime.UtcNow.AddHours(24));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        user.EmailVerificationToken.Should().BeNull();
        user.EmailVerificationTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserInRepository()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var token = "valid-token";
        var command = new VerifyEmailCommand(applicationId, email, token);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.SetEmailVerificationToken(token, DateTime.UtcNow.AddHours(24));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        _endUserRepository.Received(1).Update(user);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var applicationId = ApplicationId.CreateUnique();
        var email = Email.Create("user@example.com");
        var token = "valid-token";
        var command = new VerifyEmailCommand(applicationId, email, token);

        var user = EndUser.Create(
            applicationId,
            email,
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.SetEmailVerificationToken(token, DateTime.UtcNow.AddHours(24));

        _endUserRepository.GetByEmailAsync(applicationId, email, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Commands.SendVerificationEmail;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class SendVerificationEmailCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendVerificationEmailCommandHandler> _logger;
    private readonly SendVerificationEmailCommandHandler _handler;

    public SendVerificationEmailCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<SendVerificationEmailCommandHandler>>();

        _handler = new SendVerificationEmailCommandHandler(
            _endUserRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithUnverifiedUser_ShouldSendVerificationEmail()
    {
        var userId = EndUserId.CreateUnique();
        var command = new SendVerificationEmailCommand(userId);

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
        result.Value.Message.Should().Contain("sent successfully");
        user.EmailVerificationToken.Should().NotBeNullOrEmpty();
        user.EmailVerificationTokenExpiresAt.Should().NotBeNull();
        user.EmailVerificationTokenExpiresAt.Should().BeAfter(DateTime.UtcNow.AddHours(23));
        _endUserRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new SendVerificationEmailCommand(userId);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithAlreadyVerifiedEmail_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var command = new SendVerificationEmailCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.VerifyEmail(); 

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.EmailAlreadyVerified);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewVerificationToken()
    {
        var userId = EndUserId.CreateUnique();
        var command = new SendVerificationEmailCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        user.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();
        user.EmailVerificationToken!.Length.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task Handle_ShouldSetExpiration24HoursInFuture()
    {
        var userId = EndUserId.CreateUnique();
        var command = new SendVerificationEmailCommand(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var beforeCall = DateTime.UtcNow;
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        await _handler.Handle(command, CancellationToken.None);

        var afterCall = DateTime.UtcNow;
        user.EmailVerificationTokenExpiresAt.Should().NotBeNull();
        user.EmailVerificationTokenExpiresAt.Should().BeAfter(beforeCall.AddHours(23));
        user.EmailVerificationTokenExpiresAt.Should().BeBefore(afterCall.AddHours(25));
    }
}

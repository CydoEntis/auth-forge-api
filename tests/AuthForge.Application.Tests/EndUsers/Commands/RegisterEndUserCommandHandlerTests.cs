using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Application.EndUsers.Commands.Register;
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

public class RegisterEndUserCommandHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailParser _emailParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterEndUserCommandHandler> _logger;
    private readonly RegisterEndUserCommandHandler _handler;

    public RegisterEndUserCommandHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _emailParser = Substitute.For<IEmailParser>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<RegisterEndUserCommandHandler>>();

        _handler = new RegisterEndUserCommandHandler(
            _endUserRepository,
            _applicationRepository,
            _passwordHasher,
            _emailParser,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUser()
    {
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        var email = Email.Create("user@example.com");
        var hashedPassword = HashedPassword.Create("Password123!");

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);
        _emailParser.ParseForAuthentication(command.Email)
            .Returns(Result<Email>.Success(email));
        _endUserRepository.ExistsAsync(Arg.Any<ApplicationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.HashPassword(command.Password)
            .Returns(hashedPassword);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("user@example.com");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.Message.Should().Contain("registered successfully");

        await _endUserRepository.Received(1).AddAsync(Arg.Any<EndUser>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidApplicationId_ShouldReturnFailure()
    {
        var command = new RegisterEndUserCommand(
            "not-a-guid",
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationErrors.InvalidGuid("ApplicationId"));
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnFailure()
    {
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns((AuthForge.Domain.Entities.Application?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInactiveApplication_ShouldReturnFailure()
    {
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        application.Deactivate();

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Inactive);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "invalid-email",
            "Password123!",
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        var emailError = new AuthForge.Domain.Errors.Error("EmailParsing", "Invalid email format");

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);
        _emailParser.ParseForAuthentication(command.Email)
            .Returns(Result<Email>.Failure(emailError));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(emailError);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        var email = Email.Create("user@example.com");

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);
        _emailParser.ParseForAuthentication(command.Email)
            .Returns(Result<Email>.Success(email));
        _endUserRepository.ExistsAsync(Arg.Any<ApplicationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.DuplicateEmail);
    }

    [Fact]
    public async Task Handle_ShouldHashPasswordBeforeStoringUser()
    {
        var appId = Guid.NewGuid();
        var rawPassword = "Password123!";
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            rawPassword,
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        var email = Email.Create("user@example.com");
        var hashedPassword = HashedPassword.Create(rawPassword);

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);
        _emailParser.ParseForAuthentication(command.Email)
            .Returns(Result<Email>.Success(email));
        _endUserRepository.ExistsAsync(Arg.Any<ApplicationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.HashPassword(rawPassword)
            .Returns(hashedPassword);

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword(rawPassword);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserWithCorrectProperties()
    {
        // ARRANGE
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        var email = Email.Create("user@example.com");
        var hashedPassword = HashedPassword.Create("Password123!");

        EndUser? capturedUser = null;
        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);
        _emailParser.ParseForAuthentication(command.Email)
            .Returns(Result<Email>.Success(email));
        _endUserRepository.ExistsAsync(Arg.Any<ApplicationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.HashPassword(command.Password)
            .Returns(hashedPassword);
        _endUserRepository.When(x => x.AddAsync(Arg.Any<EndUser>(), Arg.Any<CancellationToken>()))
            .Do(x => capturedUser = x.ArgAt<EndUser>(0));

        var result = await _handler.Handle(command, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.FirstName.Should().Be("John");
        capturedUser.LastName.Should().Be("Doe");
        capturedUser.Email.Value.Should().Be("user@example.com");
        capturedUser.IsActive.Should().BeTrue();
        capturedUser.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        var email = Email.Create("user@example.com");
        var hashedPassword = HashedPassword.Create("Password123!");

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);
        _emailParser.ParseForAuthentication(command.Email)
            .Returns(Result<Email>.Success(email));
        _endUserRepository.ExistsAsync(Arg.Any<ApplicationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.HashPassword(command.Password)
            .Returns(hashedPassword);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnUserIdInResponse()
    {
        var appId = Guid.NewGuid();
        var command = new RegisterEndUserCommand(
            appId.ToString(),
            "user@example.com",
            "Password123!",
            "John",
            "Doe");

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app");
        var email = Email.Create("user@example.com");
        var hashedPassword = HashedPassword.Create("Password123!");

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);
        _emailParser.ParseForAuthentication(command.Email)
            .Returns(Result<Email>.Success(email));
        _endUserRepository.ExistsAsync(Arg.Any<ApplicationId>(), Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.HashPassword(command.Password)
            .Returns(hashedPassword);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.UserId.Should().NotBeNullOrEmpty();
        Guid.TryParse(result.Value.UserId, out _).Should().BeTrue();
    }
}

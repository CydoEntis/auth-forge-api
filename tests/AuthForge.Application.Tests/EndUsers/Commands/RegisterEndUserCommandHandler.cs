using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Application.EndUsers.Commands.Register;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.EndUsers.Commands;

public class RegisterEndUserCommandHandlerTests
{
    private readonly Mock<IEndUserRepository> _endUserRepositoryMock;
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailParser> _emailParserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterEndUserCommandHandler _handler;

    public RegisterEndUserCommandHandlerTests()
    {
        _endUserRepositoryMock = new Mock<IEndUserRepository>();
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailParserMock = new Mock<IEmailParser>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterEndUserCommandHandler(
            _endUserRepositoryMock.Object,
            _applicationRepositoryMock.Object,
            _passwordHasherMock.Object,
            _emailParserMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldRegisterEndUser()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            AuthForgeUserId.Create(Guid.NewGuid()),
            "Test App",
            "test-app");

        var command = new RegisterEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "Password123!",
            "Jane",
            "Smith");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("user@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns(HashedPassword.FromHash("hashed", "salt"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@example.com");
        result.Value.FullName.Should().Be("Jane Smith");

        _endUserRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<EndUser>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidApplicationId_ShouldReturnValidationError()
    {
        var command = new RegisterEndUserCommand(
            "not-a-guid",
            "user@example.com",
            "Password123!",
            "Jane",
            "Smith");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnNotFoundError()
    {
        var command = new RegisterEndUserCommand(
            Guid.NewGuid().ToString(),
            "user@example.com",
            "Password123!",
            "Jane",
            "Smith");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Application?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInactiveApplication_ShouldReturnInactiveError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            AuthForgeUserId.Create(Guid.NewGuid()),
            "Test App",
            "test-app");
        application.Deactivate();

        var command = new RegisterEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "Password123!",
            "Jane",
            "Smith");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Inactive);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnDuplicateError()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            AuthForgeUserId.Create(Guid.NewGuid()),
            "Test App",
            "test-app");

        var command = new RegisterEndUserCommand(
            applicationId.Value.ToString(),
            "existing@example.com",
            "Password123!",
            "Jane",
            "Smith");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("existing@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.DuplicateEmail);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithInvalidEmail_ShouldReturnValidationError(string invalidEmail)
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            AuthForgeUserId.Create(Guid.NewGuid()),
            "Test App",
            "test-app");

        var command = new RegisterEndUserCommand(
            applicationId.Value.ToString(),
            invalidEmail,
            "Password123!",
            "Jane",
            "Smith");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Failure(ValidationErrors.InvalidEmail()));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        var applicationId = Domain.ValueObjects.ApplicationId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(
            AuthForgeUserId.Create(Guid.NewGuid()),
            "Test App",
            "test-app");

        var command = new RegisterEndUserCommand(
            applicationId.Value.ToString(),
            "user@example.com",
            "PlainPassword123!",
            "Jane",
            "Smith");

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        _emailParserMock
            .Setup(x => x.ParseForAuthentication(It.IsAny<string>()))
            .Returns(Result<Email>.Success(Email.Create("user@example.com")));

        _endUserRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword("PlainPassword123!"))
            .Returns(HashedPassword.FromHash("hashed", "salt"));

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasherMock.Verify(
            x => x.HashPassword("PlainPassword123!"),
            Times.Once);
    }
}
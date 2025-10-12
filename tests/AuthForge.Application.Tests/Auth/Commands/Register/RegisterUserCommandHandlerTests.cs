using AuthForge.Application.Auth.Commands.Register;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuthForge.Application.Tests.Auth.Commands.Register;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantValidationService> _tenantValidationServiceMock;
    private readonly Mock<IEmailParser> _emailParserMock;

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantValidationServiceMock = new Mock<ITenantValidationService>();
        _emailParserMock = new Mock<IEmailParser>();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantValidationServiceMock.Object,
            _emailParserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnSuccessResult()
    {
        var command = new RegisterUserCommand
        {
            TenantId = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "Account",
        };

        var fakeTenant = Tenant.Create(
            "Test Tenant",
            "test-tenant",
            TenantSettings.Default());

        var fakeEmail = Email.Create("test@test.com");

        // Mock tenant validation
        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Success(fakeTenant));

        // Mock email parsing
        _emailParserMock
            .Setup(x => x.ParseForRegistration(It.IsAny<string>()))
            .Returns(Result<Email>.Success(fakeEmail));

        // Mock email uniqueness check
        _userRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<TenantId>(),
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Handle_InvalidTenantId_ReturnsFailure()
    {
        var command = new RegisterUserCommand
        {
            TenantId = "definitely-not-a-guid-12345",
            Email = "test@test.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "Account",
        };

        // Mock tenant validation to return failure
        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Failure(
                DomainErrors.Validation.InvalidGuid("Tenant ID")));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("tenant ID is not a valid GUID");
        result.Error.Code.Should().Be("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsFailure()
    {
        var command = new RegisterUserCommand
        {
            TenantId = Guid.NewGuid().ToString(),
            Email = "not-an-email",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "Account",
        };

        var activeTenant = Tenant.Create("Test", "test-tenant", TenantSettings.Default());

        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Success(activeTenant));

        // Mock email parsing to return failure
        _emailParserMock
            .Setup(x => x.ParseForRegistration(It.IsAny<string>()))
            .Returns(Result<Email>.Failure(DomainErrors.Validation.InvalidEmail()));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("email format is invalid");
        result.Error.Code.Should().Be("Validation.InvalidEmail");
    }

    [Fact]
    public async Task Handle_WeakPassword_ReturnsFailure()
    {
        var command = new RegisterUserCommand
        {
            TenantId = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            Password = "weak",
            FirstName = "Test",
            LastName = "Account",
        };

        var activeTenant = Tenant.Create("Test", "test-tenant", TenantSettings.Default());
        var fakeEmail = Email.Create("test@test.com");

        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Success(activeTenant));

        _emailParserMock
            .Setup(x => x.ParseForRegistration(It.IsAny<string>()))
            .Returns(Result<Email>.Success(fakeEmail));

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<TenantId>(),
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("password is too weak");
        result.Error.Code.Should().Be("Validation.InvalidPassword");
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryMethodsCorrectly()
    {
        var command = new RegisterUserCommand
        {
            TenantId = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "Account"
        };

        var activeTenant = Tenant.Create("Test", "test", TenantSettings.Default());
        var fakeEmail = Email.Create("test@test.com");

        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Success(activeTenant));

        _emailParserMock
            .Setup(x => x.ParseForRegistration(It.IsAny<string>()))
            .Returns(Result<Email>.Success(fakeEmail));

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<TenantId>(),
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _tenantValidationServiceMock.Verify(
            x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _emailParserMock.Verify(
            x => x.ParseForRegistration(It.IsAny<string>()),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.ExistsAsync(
                It.IsAny<TenantId>(),
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveTenant_ReturnsFailure()
    {
        var command = new RegisterUserCommand
        {
            TenantId = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "Account"
        };

        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Failure(DomainErrors.Tenant.Inactive));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.Inactive");

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure()
    {
        var command = new RegisterUserCommand
        {
            TenantId = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "Account"
        };

        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Failure(DomainErrors.Tenant.NotFound));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.NotFound");

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsFailure()
    {
        var command = new RegisterUserCommand
        {
            TenantId = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "Account"
        };

        var activeTenant = Tenant.Create("Test", "test", TenantSettings.Default());
        var fakeEmail = Email.Create("test@test.com");

        _tenantValidationServiceMock
            .Setup(x => x.ValidateTenantAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Tenant>.Success(activeTenant));

        _emailParserMock
            .Setup(x => x.ParseForRegistration(It.IsAny<string>()))
            .Returns(Result<Email>.Success(fakeEmail));

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(
                It.IsAny<TenantId>(),
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
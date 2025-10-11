using AuthForge.Application.Auth.Commands.Register;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AuthForge.Application.Tests.Auth.Commands.Register;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _tenantRepositoryMock.Object,
            _unitOfWorkMock.Object);
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

        _tenantRepositoryMock
            .Setup(x => x.GetByIdAsync(
                It.IsAny<TenantId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeTenant);

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

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("tenant ID is not a valid GUID");
        result.Error.Code.Should().Be("Tenant.InvalidId");
        result.Error.Message.Should().Contain("valid GUID");

        _tenantRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "should fail before calling database");
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
        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTenant);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("email format is invalid");
        result.Error.Code.Should().Be("User.InvalidEmail");
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
        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTenant);

        _userRepositoryMock.Setup(x =>
                x.ExistsAsync(
                    It.IsAny<TenantId>(),
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("password is too weak");
        result.Error.Code.Should().Be("User.InvalidPassword");
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

        _tenantRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTenant);

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<TenantId>(), It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _tenantRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.ExistsAsync(It.IsAny<TenantId>(), It.IsAny<Email>(), It.IsAny<CancellationToken>()),
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

        var inactiveTenant = Tenant.Create("Test", "test", TenantSettings.Default());
        inactiveTenant.Deactivate();

        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveTenant);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.Inactive");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
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

        _tenantRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

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

        _tenantRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTenant);

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<TenantId>(), It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
using AuthForge.Application.Applications.Commands.Create;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.Applications.Commands;

public class CreateApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IAuthForgeUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateApplicationCommandHandler _handler;

    public CreateApplicationCommandHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _userRepositoryMock = new Mock<IAuthForgeUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateApplicationCommandHandler(
            _applicationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateApplication()
    {
        var userId = Guid.NewGuid();
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        var command = new CreateApplicationCommand(userId.ToString(), "My Awesome App");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _applicationRepositoryMock
            .Setup(x => x.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Awesome App");
        result.Value.Slug.Should().Be("my-awesome-app");
        result.Value.IsActive.Should().BeTrue();

        _applicationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Application>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldReturnValidationError()
    {
        var command = new CreateApplicationCommand("not-a-guid", "My App");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnNotFoundError()
    {
        var command = new CreateApplicationCommand(Guid.NewGuid().ToString(), "My App");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthForgeUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnInactiveError()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        user.Deactivate();

        var command = new CreateApplicationCommand(user.Id.Value.ToString(), "My App");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthForgeUserErrors.Inactive);
    }

    [Fact]
    public async Task Handle_WithDuplicateSlug_ShouldAppendGuidToSlug()
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        var command = new CreateApplicationCommand(user.Id.Value.ToString(), "My App");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _applicationRepositoryMock
            .Setup(x => x.SlugExistsAsync("my-app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().StartWith("my-app-");
        result.Value.Slug.Should().NotBe("my-app");
    }

    [Theory]
    [InlineData("Test App", "test-app")]
    [InlineData("My Awesome App", "my-awesome-app")]
    [InlineData("App With   Spaces", "app-with-spaces")]
    [InlineData("App_With_Underscores", "app-with-underscores")]
    public async Task Handle_ShouldGenerateCorrectSlug(string appName, string expectedSlug)
    {
        var user = AuthForgeUser.Create(
            Email.Create("test@example.com"),
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");
        var command = new CreateApplicationCommand(user.Id.Value.ToString(), appName);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _applicationRepositoryMock
            .Setup(x => x.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().Be(expectedSlug);
    }
}
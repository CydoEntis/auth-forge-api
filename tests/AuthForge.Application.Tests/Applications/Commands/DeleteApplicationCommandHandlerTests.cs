using AuthForge.Application.Applications.Commands.Delete;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.Applications.Commands;

public class DeleteApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteApplicationCommandHandler _handler;

    public DeleteApplicationCommandHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteApplicationCommandHandler(
            _applicationRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldDeactivateApplication()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(userId, "Test App", "test-app");
        var command = new DeleteApplicationCommand(
            application.Id.Value.ToString(),
            userId.Value.ToString());

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        application.IsActive.Should().BeFalse();
        application.DeactivatedAtUtc.Should().NotBeNull();

        _applicationRepositoryMock.Verify(
            x => x.Update(It.IsAny<Domain.Entities.Application>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidApplicationId_ShouldReturnValidationError()
    {
        var command = new DeleteApplicationCommand("not-a-guid", Guid.NewGuid().ToString());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldReturnValidationError()
    {
        var command = new DeleteApplicationCommand(Guid.NewGuid().ToString(), "not-a-guid");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnNotFoundError()
    {
        var command = new DeleteApplicationCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString());

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Application?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnUnauthorizedError()
    {
        var ownerId = AuthForgeUserId.Create(Guid.NewGuid());
        var otherUserId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(ownerId, "Test App", "test-app");
        var command = new DeleteApplicationCommand(
            application.Id.Value.ToString(),
            otherUserId.Value.ToString());

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInactive_ShouldReturnSuccess()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(userId, "Test App", "test-app");
        application.Deactivate();
        var command = new DeleteApplicationCommand(
            application.Id.Value.ToString(),
            userId.Value.ToString());

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
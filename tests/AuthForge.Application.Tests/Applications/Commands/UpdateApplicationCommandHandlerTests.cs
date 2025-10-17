using AuthForge.Application.Applications.Commands.Update;
using AuthForge.Application.Applications.Models;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.Applications.Commands;

public class UpdateApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateApplicationCommandHandler _handler;

    public UpdateApplicationCommandHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateApplicationCommandHandler(
            _applicationRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateApplication()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(userId, "Original Name", "original-slug");
        var settings = new AppSettings(3, 30, 60, 14);
        var command = new UpdateApplicationCommand(
            application.Id.Value.ToString(),
            userId.Value.ToString(),
            "Updated Name",
            settings);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        result.Value.IsActive.Should().BeTrue();

        application.Name.Should().Be("Updated Name");
        application.Settings.MaxFailedLoginAttempts.Should().Be(3);
        application.Settings.LockoutDurationMinutes.Should().Be(30);

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
        var settings = new AppSettings(3, 30, 60, 14);
        var command = new UpdateApplicationCommand(
            "not-a-guid",
            Guid.NewGuid().ToString(),
            "Updated Name",
            settings);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldReturnValidationError()
    {
        var settings = new AppSettings(3, 30, 60, 14);
        var command = new UpdateApplicationCommand(
            Guid.NewGuid().ToString(),
            "not-a-guid",
            "Updated Name",
            settings);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnNotFoundError()
    {
        var settings = new AppSettings(3, 30, 60, 14);
        var command = new UpdateApplicationCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "Updated Name",
            settings);

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
        var application = Domain.Entities.Application.Create(ownerId, "Original Name", "original-slug");
        var settings = new AppSettings(3, 30, 60, 14);
        var command = new UpdateApplicationCommand(
            application.Id.Value.ToString(),
            otherUserId.Value.ToString(),
            "Updated Name",
            settings);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_WithInactiveApplication_ShouldReturnInactiveError()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(userId, "Original Name", "original-slug");
        application.Deactivate();
        var settings = new AppSettings(3, 30, 60, 14);
        var command = new UpdateApplicationCommand(
            application.Id.Value.ToString(),
            userId.Value.ToString(),
            "Updated Name",
            settings);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Inactive);
    }
}
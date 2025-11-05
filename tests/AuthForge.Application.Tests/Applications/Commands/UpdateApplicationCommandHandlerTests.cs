using AuthForge.Application.Applications.Commands.UpdateApplication;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.Applications.Commands;

public class UpdateApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UpdateApplicationCommandHandler>> _loggerMock;
    private readonly UpdateApplicationCommandHandler _handler;

    public UpdateApplicationCommandHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UpdateApplicationCommandHandler>>();

        _handler = new UpdateApplicationCommandHandler(
            _applicationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateApplication()
    {
        var application = Domain.Entities.Application.Create("Original Name", "original-slug");
        var command = new UpdateApplicationCommand(
            application.Id.Value.ToString(),
            "Updated Name",
            null,
            null,
            null,
            null,
            null);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        result.Value.IsActive.Should().BeTrue();

        application.Name.Should().Be("Updated Name");

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
        var command = new UpdateApplicationCommand(
            "not-a-guid",
            "Updated Name",
            null,
            null,
            null,
            null,
            null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnNotFoundError()
    {
        var command = new UpdateApplicationCommand(
            Guid.NewGuid().ToString(),
            "Updated Name",
            null,
            null,
            null,
            null,
            null);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Application?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInactiveApplication_ShouldUpdateSuccessfully()
    {
        var application = Domain.Entities.Application.Create("Original Name", "original-slug");
        application.Deactivate();
        var command = new UpdateApplicationCommand(
            application.Id.Value.ToString(),
            "Updated Name",
            null,
            null,
            null,
            null,
            null);

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        application.Name.Should().Be("Updated Name");

        _applicationRepositoryMock.Verify(
            x => x.Update(It.IsAny<Domain.Entities.Application>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
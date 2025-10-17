using AuthForge.Application.Applications.Queries.GetById;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.Applications.Queries;

public class GetApplicationByIdQueryHandlerTests
{
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly GetApplicationByIdQueryHandler _handler;

    public GetApplicationByIdQueryHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _handler = new GetApplicationByIdQueryHandler(_applicationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnApplicationDetail()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(userId, "Test App", "test-app");
        var query = new GetApplicationByIdQuery(
            application.Id.Value.ToString(),
            userId.Value.ToString());

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ApplicationId.Should().Be(application.Id.Value.ToString());
        result.Value.Name.Should().Be("Test App");
        result.Value.Slug.Should().Be("test-app");
        result.Value.IsActive.Should().BeTrue();
        result.Value.Settings.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidApplicationId_ShouldReturnValidationError()
    {
        var query = new GetApplicationByIdQuery("not-a-guid", Guid.NewGuid().ToString());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldReturnValidationError()
    {
        var query = new GetApplicationByIdQuery(Guid.NewGuid().ToString(), "not-a-guid");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnNotFoundError()
    {
        var query = new GetApplicationByIdQuery(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString());

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Application?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnUnauthorizedError()
    {
        var ownerId = AuthForgeUserId.Create(Guid.NewGuid());
        var otherUserId = AuthForgeUserId.Create(Guid.NewGuid());
        var application = Domain.Entities.Application.Create(ownerId, "Test App", "test-app");
        var query = new GetApplicationByIdQuery(
            application.Id.Value.ToString(),
            otherUserId.Value.ToString());

        _applicationRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Domain.ValueObjects.ApplicationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Unauthorized);
    }
}
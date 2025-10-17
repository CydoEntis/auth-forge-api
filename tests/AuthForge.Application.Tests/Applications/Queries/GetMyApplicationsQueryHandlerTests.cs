using AuthForge.Application.Applications.Models;
using AuthForge.Application.Applications.Queries.GetMy;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.Applications.Queries;

public class GetMyApplicationsQueryHandlerTests
{
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly GetMyApplicationsQueryHandler _handler;

    public GetMyApplicationsQueryHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _handler = new GetMyApplicationsQueryHandler(_applicationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnPagedApplications()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var app1 = Domain.Entities.Application.Create("App 1", "app-1");
        var app2 = Domain.Entities.Application.Create("App 2", "app-2");
        var applications = new List<Domain.Entities.Application> { app1, app2 };

        var query = new GetMyApplicationsQuery
        {
            UserId = userId.Value.ToString(),
            PageNumber = 1,
            PageSize = 10,
            SortBy = ApplicationSortField.CreatedAt,
            SortOrder = Common.Models.SortOrder.Desc
        };

        _applicationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldReturnValidationError()
    {
        var query = new GetMyApplicationsQuery
        {
            UserId = "not-a-guid",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation.InvalidGuid");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var applications = new List<Domain.Entities.Application>();
        for (int i = 0; i < 25; i++)
        {
            applications.Add(Domain.Entities.Application.Create($"App {i}", $"app-{i}"));
        }

        var query = new GetMyApplicationsQuery
        {
            UserId = userId.Value.ToString(),
            PageNumber = 2,
            PageSize = 10,
            SortBy = ApplicationSortField.CreatedAt,
            SortOrder = Common.Models.SortOrder.Desc
        };

        _applicationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.PageNumber.Should().Be(2);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterResults()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var app1 = Domain.Entities.Application.Create("E-commerce App", "e-commerce-app");
        var app2 = Domain.Entities.Application.Create("Blog Platform", "blog-platform");
        var app3 = Domain.Entities.Application.Create("Commerce API", "commerce-api");
        var applications = new List<Domain.Entities.Application> { app1, app2, app3 };

        var query = new GetMyApplicationsQuery
        {
            UserId = userId.Value.ToString(),
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "commerce",
            SortBy = ApplicationSortField.Name,
            SortOrder = Common.Models.SortOrder.Asc
        };

        _applicationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(x =>
            x.Name.Contains("commerce", StringComparison.OrdinalIgnoreCase) ||
            x.Slug.Contains("commerce", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_WithSortByName_ShouldSortCorrectly()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var app1 = Domain.Entities.Application.Create("Zebra App", "zebra-app");
        var app2 = Domain.Entities.Application.Create("Alpha App", "alpha-app");
        var app3 = Domain.Entities.Application.Create("Beta App", "beta-app");
        var applications = new List<Domain.Entities.Application> { app1, app2, app3 };

        var query = new GetMyApplicationsQuery
        {
            UserId = userId.Value.ToString(),
            PageNumber = 1,
            PageSize = 10,
            SortBy = ApplicationSortField.Name,
            SortOrder = Common.Models.SortOrder.Asc
        };

        _applicationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items[0].Name.Should().Be("Alpha App");
        result.Value.Items[1].Name.Should().Be("Beta App");
        result.Value.Items[2].Name.Should().Be("Zebra App");
    }

    [Fact]
    public async Task Handle_WithEmptyResults_ShouldReturnEmptyPage()
    {
        var userId = AuthForgeUserId.Create(Guid.NewGuid());
        var applications = new List<Domain.Entities.Application>();

        var query = new GetMyApplicationsQuery
        {
            UserId = userId.Value.ToString(),
            PageNumber = 1,
            PageSize = 10
        };

        _applicationRepositoryMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<AuthForgeUserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }
}
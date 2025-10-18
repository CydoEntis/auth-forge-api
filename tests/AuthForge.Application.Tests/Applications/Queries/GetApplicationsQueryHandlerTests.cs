using AuthForge.Application.Applications.Enums;
using AuthForge.Application.Applications.Queries.GetAll;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using FluentAssertions;
using NSubstitute;

namespace AuthForge.Application.Tests.Applications.Queries;

public sealed class GetApplicationsQueryHandlerTests
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly GetApplicationsQueryHandler _handler;

    public GetApplicationsQueryHandlerTests()
    {
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _handler = new GetApplicationsQueryHandler(_applicationRepository);
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnsPagedApplications()
    {
        var applications = new List<AuthForge.Domain.Entities.Application>
        {
            AuthForge.Domain.Entities.Application.Create("App 1", "app-1"),
            AuthForge.Domain.Entities.Application.Create("App 2", "app-2"),
            AuthForge.Domain.Entities.Application.Create("App 3", "app-3")
        };

        _applicationRepository
            .GetPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<ApplicationSortBy>(),
                Arg.Any<SortOrder>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((applications, 3));

        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        var query = new GetApplicationsQuery(parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_PassesSearchTermToRepository()
    {
        var applications = new List<AuthForge.Domain.Entities.Application>
        {
            AuthForge.Domain.Entities.Application.Create("Test App", "test-app")
        };

        _applicationRepository
            .GetPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<ApplicationSortBy>(),
                Arg.Any<SortOrder>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((applications, 1));

        var parameters = new ApplicationFilterParameters
        {
            SearchTerm = "test",
            PageNumber = 1,
            PageSize = 10
        };

        var query = new GetApplicationsQuery(parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        await _applicationRepository.Received(1).GetPagedAsync(
            "test",
            Arg.Any<bool?>(),
            Arg.Any<ApplicationSortBy>(),
            Arg.Any<SortOrder>(),
            1,
            10,
            Arg.Any<CancellationToken>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithIsActiveFilter_PassesFilterToRepository()
    {
        var applications = new List<AuthForge.Domain.Entities.Application>
        {
            AuthForge.Domain.Entities.Application.Create("Active App", "active-app")
        };

        _applicationRepository
            .GetPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<ApplicationSortBy>(),
                Arg.Any<SortOrder>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((applications, 1));

        var parameters = new ApplicationFilterParameters
        {
            IsActive = true,
            PageNumber = 1,
            PageSize = 10
        };

        var query = new GetApplicationsQuery(parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        await _applicationRepository.Received(1).GetPagedAsync(
            Arg.Any<string?>(),
            true,
            Arg.Any<ApplicationSortBy>(),
            Arg.Any<SortOrder>(),
            1,
            10,
            Arg.Any<CancellationToken>());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithCustomSorting_PassesSortingToRepository()
    {
        var applications = new List<AuthForge.Domain.Entities.Application>
        {
            AuthForge.Domain.Entities.Application.Create("App A", "app-a"),
            AuthForge.Domain.Entities.Application.Create("App B", "app-b")
        };

        _applicationRepository
            .GetPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<ApplicationSortBy>(),
                Arg.Any<SortOrder>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((applications, 2));

        var parameters = new ApplicationFilterParameters
        {
            SortBy = ApplicationSortBy.Name,
            SortOrder = SortOrder.Asc,
            PageNumber = 1,
            PageSize = 10
        };

        var query = new GetApplicationsQuery(parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        await _applicationRepository.Received(1).GetPagedAsync(
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            ApplicationSortBy.Name,
            SortOrder.Asc,
            1,
            10,
            Arg.Any<CancellationToken>());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithPagination_CalculatesCorrectTotalPages()
    {
        var applications = new List<AuthForge.Domain.Entities.Application>
        {
            AuthForge.Domain.Entities.Application.Create("App 1", "app-1"),
            AuthForge.Domain.Entities.Application.Create("App 2", "app-2")
        };

        _applicationRepository
            .GetPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<ApplicationSortBy>(),
                Arg.Any<SortOrder>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((applications, 25));

        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        var query = new GetApplicationsQuery(parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithEmptyResults_ReturnsEmptyPagedResponse()
    {
        _applicationRepository
            .GetPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<ApplicationSortBy>(),
                Arg.Any<SortOrder>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((new List<AuthForge.Domain.Entities.Application>(), 0));

        var parameters = new ApplicationFilterParameters
        {
            SearchTerm = "nonexistent",
            PageNumber = 1,
            PageSize = 10
        };

        var query = new GetApplicationsQuery(parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MapsApplicationToSummaryCorrectly()
    {
        var application = AuthForge.Domain.Entities.Application.Create("My Application", "my-application");

        _applicationRepository
            .GetPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<bool?>(),
                Arg.Any<ApplicationSortBy>(),
                Arg.Any<SortOrder>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns((new List<AuthForge.Domain.Entities.Application> { application }, 1));

        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        var query = new GetApplicationsQuery(parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var summary = result.Value.Items.First();
        summary.ApplicationId.Should().Be(application.Id.Value.ToString());
        summary.Name.Should().Be("My Application");
        summary.Slug.Should().Be("my-application");
        summary.IsActive.Should().BeTrue();
        summary.EndUserCount.Should().Be(0);
        summary.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
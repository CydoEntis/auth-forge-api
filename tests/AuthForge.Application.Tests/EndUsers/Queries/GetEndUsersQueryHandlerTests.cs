using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Enums;
using AuthForge.Application.EndUsers.Models;
using AuthForge.Application.EndUsers.Queries.GetAll;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Queries;

public class GetEndUsersQueryHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly GetEndUsersQueryHandler _handler;

    public GetEndUsersQueryHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _applicationRepository = Substitute.For<IApplicationRepository>();

        _handler = new GetEndUsersQueryHandler(
            _endUserRepository,
            _applicationRepository);
    }

    [Fact]
    public async Task Handle_WithValidApplicationId_ShouldReturnPagedUsers()
    {
        var applicationId = ApplicationId.CreateUnique();
        var parameters = new EndUserFilterParameters
        {
            PageNumber = 1,
            PageSize = 10
        };
        var query = new GetEndUsersQuery(applicationId.Value.ToString(), parameters);

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var users = new List<EndUser>
        {
            EndUser.Create(applicationId, Email.Create("user1@example.com"), HashedPassword.Create("Pass123!"), "John", "Doe"),
            EndUser.Create(applicationId, Email.Create("user2@example.com"), HashedPassword.Create("Pass123!"), "Jane", "Smith")
        };

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _endUserRepository.GetPagedAsync(
            applicationId,
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<EndUserSortBy>(),
            Arg.Any<SortOrder>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((users, 2));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithInvalidApplicationId_ShouldReturnFailure()
    {
        var parameters = new EndUserFilterParameters { PageNumber = 1, PageSize = 10 };
        var query = new GetEndUsersQuery("invalid-guid", parameters);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationErrors.InvalidGuid("ApplicationId"));
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnFailure()
    {
        var applicationId = Guid.NewGuid();
        var parameters = new EndUserFilterParameters { PageNumber = 1, PageSize = 10 };
        var query = new GetEndUsersQuery(applicationId.ToString(), parameters);

        _applicationRepository.GetByIdAsync(Arg.Any<ApplicationId>(), Arg.Any<CancellationToken>())
            .Returns((AuthForge.Domain.Entities.Application?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldUseDefaultPaginationWhenNotProvided()
    {
        var applicationId = ApplicationId.CreateUnique();
        var parameters = new EndUserFilterParameters(); 
        var query = new GetEndUsersQuery(applicationId.Value.ToString(), parameters);

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var users = new List<EndUser>();

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _endUserRepository.GetPagedAsync(
            applicationId,
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<EndUserSortBy>(),
            Arg.Any<SortOrder>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((users, 0));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(1); 
        result.Value.PageSize.Should().Be(10); 
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPagesCorrectly()
    {
        // ARRANGE
        var applicationId = ApplicationId.CreateUnique();
        var parameters = new EndUserFilterParameters { PageNumber = 1, PageSize = 3 };
        var query = new GetEndUsersQuery(applicationId.Value.ToString(), parameters);

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var users = new List<EndUser>
        {
            EndUser.Create(applicationId, Email.Create("user1@example.com"), HashedPassword.Create("Pass123!"), "User", "One"),
            EndUser.Create(applicationId, Email.Create("user2@example.com"), HashedPassword.Create("Pass123!"), "User", "Two"),
            EndUser.Create(applicationId, Email.Create("user3@example.com"), HashedPassword.Create("Pass123!"), "User", "Three")
        };

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _endUserRepository.GetPagedAsync(
            applicationId,
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<EndUserSortBy>(),
            Arg.Any<SortOrder>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((users, 10)); 

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPages.Should().Be(4); 
    }
}

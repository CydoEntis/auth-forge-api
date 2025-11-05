using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Queries.GetById;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Queries;

public class GetEndUserByIdQueryHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly GetEndUserByIdQueryHandler _handler;

    public GetEndUserByIdQueryHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();
        _applicationRepository = Substitute.For<IApplicationRepository>();

        _handler = new GetEndUserByIdQueryHandler(
            _endUserRepository,
            _applicationRepository);
    }

    [Fact]
    public async Task Handle_WithValidUserAndApplication_ShouldReturnUser()
    {
        var applicationId = ApplicationId.CreateUnique();
        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var user = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        var query = new GetEndUserByIdQuery(applicationId, user.Id);

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _endUserRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id.Value.ToString());
        result.Value.Email.Should().Be("user@example.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var applicationId = ApplicationId.CreateUnique();
        var query = new GetEndUserByIdQuery(applicationId, userId);

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns((AuthForge.Domain.Entities.Application?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var applicationId = ApplicationId.CreateUnique();
        var query = new GetEndUserByIdQuery(applicationId, userId);

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithUserFromDifferentApplication_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var applicationId = ApplicationId.CreateUnique();
        var differentApplicationId = ApplicationId.CreateUnique();
        var query = new GetEndUserByIdQuery(applicationId, userId);

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var user = EndUser.Create(
            differentApplicationId, 
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.UserNotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUserDetails()
    {
        var userId = EndUserId.CreateUnique();
        var applicationId = ApplicationId.CreateUnique();
        var query = new GetEndUserByIdQuery(applicationId, userId);

        var application = AuthForge.Domain.Entities.Application.Create("Test App", "test-app", null, null);
        var user = EndUser.Create(
            applicationId,
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _applicationRepository.GetByIdAsync(applicationId, Arg.Any<CancellationToken>())
            .Returns(application);
        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.UserId.Should().NotBeNullOrEmpty();
        result.Value.Email.Should().NotBeNullOrEmpty();
        result.Value.FirstName.Should().NotBeNullOrEmpty();
        result.Value.LastName.Should().NotBeNullOrEmpty();
        result.Value.IsEmailVerified.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
        result.Value.FailedLoginAttempts.Should().Be(0);
        result.Value.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}

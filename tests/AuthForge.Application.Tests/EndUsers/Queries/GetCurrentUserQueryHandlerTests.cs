using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.EndUsers.Queries.GetCurrentUser;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.EndUsers.Queries;

public class GetCurrentUserQueryHandlerTests
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _endUserRepository = Substitute.For<IEndUserRepository>();

        _handler = new GetCurrentUserQueryHandler(_endUserRepository);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnCurrentUser()
    {
        var userId = EndUserId.CreateUnique();
        var query = new GetCurrentUserQuery(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id.Value.ToString());
        result.Value.Email.Should().Be("user@example.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.IsEmailVerified.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = EndUserId.CreateUnique();
        var query = new GetCurrentUserQuery(userId);

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((EndUser?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EndUserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUserProperties()
    {
        var userId = EndUserId.CreateUnique();
        var query = new GetCurrentUserQuery(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.UserId.Should().NotBeNullOrEmpty();
        result.Value.Email.Should().NotBeNullOrEmpty();
        result.Value.FirstName.Should().NotBeNullOrEmpty();
        result.Value.LastName.Should().NotBeNullOrEmpty();
        result.Value.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Handle_WithVerifiedUser_ShouldShowVerifiedStatus()
    {
        var userId = EndUserId.CreateUnique();
        var query = new GetCurrentUserQuery(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.VerifyEmail(); 

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDeactivatedUser_ShouldShowInactiveStatus()
    {
        var userId = EndUserId.CreateUnique();
        var query = new GetCurrentUserQuery(userId);

        var user = EndUser.Create(
            ApplicationId.CreateUnique(),
            Email.Create("user@example.com"),
            HashedPassword.Create("Password123!"),
            "John",
            "Doe");
        user.Deactivate(); 

        _endUserRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Value.IsActive.Should().BeFalse();
    }
}

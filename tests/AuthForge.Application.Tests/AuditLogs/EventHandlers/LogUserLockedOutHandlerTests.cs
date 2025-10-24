using AuthForge.Application.AuditLogs.EventHandlers;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Constants;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Events;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Tests.AuditLogs.EventHandlers;

public class LogUserLockedOutHandlerTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IEndUserRepository> _endUserRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LogUserLockedOutHandler _handler;

    public LogUserLockedOutHandlerTests()
    {
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _endUserRepositoryMock = new Mock<IEndUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LogUserLockedOutHandler(
            _auditLogRepositoryMock.Object,
            _endUserRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldCreateAuditLog()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());
        var lockedUntil = DateTime.UtcNow.AddMinutes(15);
        var failedAttempts = 5;

        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        var domainEvent = new EndUserLockedOutDomainEvent(userId, lockedUntil, failedAttempts);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        _auditLogRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCreateAuditLog()
    {
        // ARRANGE
        var userId = EndUserId.Create(Guid.NewGuid());
        var lockedUntil = DateTime.UtcNow.AddMinutes(15);
        var domainEvent = new EndUserLockedOutDomainEvent(userId, lockedUntil, 5);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUser?)null);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        _auditLogRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSerializeEventDetailsCorrectly()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("testuser@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());
        var lockedUntil = DateTime.UtcNow.AddMinutes(30);
        var failedAttempts = 7;

        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "Jane",
            "Smith");

        var domainEvent = new EndUserLockedOutDomainEvent(userId, lockedUntil, failedAttempts);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        capturedAuditLog.Should().NotBeNull();
        capturedAuditLog!.Details.Should().Contain(email.Value);
        capturedAuditLog!.Details.Should().Contain(userId.Value.ToString());
        capturedAuditLog!.Details.Should().Contain(failedAttempts.ToString());
        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.UserLockedOut);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Handle_ShouldCaptureFailedAttemptsCount(int failedAttempts)
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());
        var lockedUntil = DateTime.UtcNow.AddMinutes(15);

        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        var domainEvent = new EndUserLockedOutDomainEvent(userId, lockedUntil, failedAttempts);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        capturedAuditLog!.Details.Should().Contain($"\"failedAttempts\":{failedAttempts}");
    }
}

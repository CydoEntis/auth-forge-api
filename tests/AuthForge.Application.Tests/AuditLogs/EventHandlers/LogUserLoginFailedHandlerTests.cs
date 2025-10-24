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

public class LogUserLoginFailedHandlerTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LogUserLoginFailedHandler _handler;

    public LogUserLoginFailedHandlerTests()
    {
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LogUserLoginFailedHandler(
            _auditLogRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldCreateAuditLog()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserLoginFailedDomainEvent(
            userId,
            applicationId,
            email,
            3);

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
    public async Task Handle_ShouldSerializeEventDetailsCorrectly()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("testuser@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());
        var failedAttempts = 5;

        var domainEvent = new EndUserLoginFailedDomainEvent(
            userId,
            applicationId,
            email,
            failedAttempts);

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
        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.UserLoginFailed);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectApplicationId()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserLoginFailedDomainEvent(
            userId,
            applicationId,
            email,
            1);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        capturedAuditLog!.ApplicationId.Should().Be(applicationId);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectEventType()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserLoginFailedDomainEvent(
            userId,
            applicationId,
            email,
            2);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.UserLoginFailed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Handle_ShouldCaptureFailedAttemptsCount(int failedAttempts)
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserLoginFailedDomainEvent(
            userId,
            applicationId,
            email,
            failedAttempts);

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

    [Fact]
    public async Task Handle_ShouldSetUserEmail()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("testuser@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserLoginFailedDomainEvent(
            userId,
            applicationId,
            email,
            1);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        capturedAuditLog!.PerformedBy.Should().Be(email.Value);
    }

    [Fact]
    public async Task Handle_ShouldSetUserId()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserLoginFailedDomainEvent(
            userId,
            applicationId,
            email,
            1);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        capturedAuditLog!.TargetUserId.Should().Be(userId.Value.ToString());
    }
}

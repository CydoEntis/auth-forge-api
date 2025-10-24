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

public class LogUserDeletedHandlerTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LogUserDeletedHandler _handler;

    public LogUserDeletedHandlerTests()
    {
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LogUserDeletedHandler(
            _auditLogRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldCreateAuditLog()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("deleted@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserDeletedDomainEvent(
            userId,
            applicationId,
            email);

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
        var email = Email.Create("deleted@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserDeletedDomainEvent(
            userId,
            applicationId,
            email);

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
        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.UserDeleted);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectEventType()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("deleted@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserDeletedDomainEvent(
            userId,
            applicationId,
            email);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        // ACT
        await _handler.Handle(domainEvent, CancellationToken.None);

        // ASSERT
        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.UserDeleted);
    }
}

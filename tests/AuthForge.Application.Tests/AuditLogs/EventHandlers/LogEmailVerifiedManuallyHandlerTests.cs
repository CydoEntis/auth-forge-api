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

public class LogEmailVerifiedManuallyHandlerTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly LogEmailVerifiedManuallyHandler _handler;

    public LogEmailVerifiedManuallyHandlerTests()
    {
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new LogEmailVerifiedManuallyHandler(
            _auditLogRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldCreateAuditLog()
    {
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserEmailVerifiedManuallyDomainEvent(userId, applicationId, email);

        await _handler.Handle(domainEvent, CancellationToken.None);

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
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("verified@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserEmailVerifiedManuallyDomainEvent(userId, applicationId, email);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        await _handler.Handle(domainEvent, CancellationToken.None);

        capturedAuditLog.Should().NotBeNull();
        capturedAuditLog!.Details.Should().Contain(email.Value);
        capturedAuditLog!.Details.Should().Contain(userId.Value.ToString());
        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.EmailVerifiedManually);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectEventType()
    {
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var domainEvent = new EndUserEmailVerifiedManuallyDomainEvent(userId, applicationId, email);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        await _handler.Handle(domainEvent, CancellationToken.None);

        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.EmailVerifiedManually);
    }
}

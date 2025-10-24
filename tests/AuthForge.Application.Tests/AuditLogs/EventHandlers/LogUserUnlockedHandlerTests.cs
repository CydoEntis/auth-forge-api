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

public class LogUserUnlockedHandlerTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IEndUserRepository> _endUserRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LogUserUnlockedHandler _handler;

    public LogUserUnlockedHandlerTests()
    {
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _endUserRepositoryMock = new Mock<IEndUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new LogUserUnlockedHandler(
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

        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        var domainEvent = new EndUserUnlockedDomainEvent(userId, applicationId);

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
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var domainEvent = new EndUserUnlockedDomainEvent(userId, applicationId);

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
    public async Task Handle_ShouldSetCorrectEventType()
    {
        // ARRANGE
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        var domainEvent = new EndUserUnlockedDomainEvent(userId, applicationId);

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
        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.UserUnlocked);
    }
}

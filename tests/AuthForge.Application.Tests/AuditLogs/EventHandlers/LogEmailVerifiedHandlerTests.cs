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

public class LogEmailVerifiedHandlerTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IEndUserRepository> _endUserRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly LogEmailVerifiedHandler _handler;

    public LogEmailVerifiedHandlerTests()
    {
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _endUserRepositoryMock = new Mock<IEndUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new LogEmailVerifiedHandler(
            _auditLogRepositoryMock.Object,
            _endUserRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidEvent_ShouldCreateAuditLog()
    {
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        var domainEvent = new EndUserEmailVerifiedDomainEvent(userId, email);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        await _handler.Handle(domainEvent, CancellationToken.None);

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
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());
        var domainEvent = new EndUserEmailVerifiedDomainEvent(userId, email);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EndUser?)null);

        await _handler.Handle(domainEvent, CancellationToken.None);

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
        var applicationId = ApplicationId.Create(Guid.NewGuid());
        var email = Email.Create("user@example.com");
        var userId = EndUserId.Create(Guid.NewGuid());

        var endUser = EndUser.Create(
            applicationId,
            email,
            HashedPassword.FromHash("hash", "salt"),
            "John",
            "Doe");

        var domainEvent = new EndUserEmailVerifiedDomainEvent(userId, email);

        _endUserRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(endUser);

        AuditLog? capturedAuditLog = null;

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        await _handler.Handle(domainEvent, CancellationToken.None);

        capturedAuditLog!.EventType.Should().Be(AuditEventConstants.EmailVerified);
    }
}

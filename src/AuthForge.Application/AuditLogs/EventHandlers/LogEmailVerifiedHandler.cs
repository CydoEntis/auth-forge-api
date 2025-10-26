using System.Text.Json;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Constants;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.AuditLogs.EventHandlers;

public sealed class LogEmailVerifiedHandler
    : INotificationHandler<EndUserEmailVerifiedDomainEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public LogEmailVerifiedHandler(
        IAuditLogRepository auditLogRepository,
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async ValueTask Handle(
        EndUserEmailVerifiedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null) return;

        var details = JsonSerializer.Serialize(new
        {
            userId = notification.UserId.Value.ToString(),
            email = notification.Email.Value
        });

        var auditLog = AuditLog.Create(
            user.ApplicationId,
            AuditEventConstants.EmailVerified,
            notification.Email.Value,
            notification.UserId.Value.ToString(),
            details,
            _currentUserService.IpAddress);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
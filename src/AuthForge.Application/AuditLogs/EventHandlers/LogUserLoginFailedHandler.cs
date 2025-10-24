using System.Text.Json;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Constants;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.AuditLogs.EventHandlers;

public sealed class LogUserLoginFailedHandler
    : INotificationHandler<EndUserLoginFailedDomainEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogUserLoginFailedHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(
        EndUserLoginFailedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var details = JsonSerializer.Serialize(new
        {
            email = notification.Email.Value,
            userId = notification.UserId.Value.ToString(),
            failedAttempts = notification.FailedAttempts
        });

        var auditLog = AuditLog.Create(
            notification.ApplicationId,
            AuditEventConstants.UserLoginFailed,
            notification.Email.Value,
            notification.UserId.Value.ToString(),
            details,
            null); 

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

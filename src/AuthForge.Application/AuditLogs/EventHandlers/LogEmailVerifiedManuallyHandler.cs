using System.Text.Json;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Constants;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.AuditLogs.EventHandlers;

public sealed class LogEmailVerifiedManuallyHandler 
    : INotificationHandler<EndUserEmailVerifiedManuallyDomainEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogEmailVerifiedManuallyHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(
        EndUserEmailVerifiedManuallyDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var details = JsonSerializer.Serialize(new
        {
            userId = notification.UserId.Value.ToString(),
            email = notification.Email.Value
        });

        var auditLog = AuditLog.Create(
            notification.ApplicationId,
            AuditEventConstants.EmailVerifiedManually,
            "admin",
            notification.UserId.Value.ToString(),
            details,
            null);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
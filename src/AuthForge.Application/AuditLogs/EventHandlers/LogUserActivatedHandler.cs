using System.Text.Json;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Constants;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.AuditLogs.EventHandlers;

public sealed class LogUserActivatedHandler
    : INotificationHandler<EndUserActivatedDomainEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogUserActivatedHandler(
        IAuditLogRepository auditLogRepository,
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask Handle(
        EndUserActivatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null) return;

        var details = JsonSerializer.Serialize(new
        {
            userId = notification.UserId.Value.ToString(),
            email = user.Email.Value
        });

        var auditLog = AuditLog.Create(
            notification.ApplicationId,
            AuditEventConstants.UserActivated,
            "admin",
            notification.UserId.Value.ToString(),
            details,
            null);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
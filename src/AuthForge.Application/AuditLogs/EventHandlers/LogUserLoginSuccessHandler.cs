using System.Text.Json;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Constants;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.AuditLogs.EventHandlers;

public sealed class LogUserLoginSuccessHandler
    : INotificationHandler<EndUserLoggedInDomainEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public LogUserLoginSuccessHandler(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async ValueTask Handle(
        EndUserLoggedInDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var details = JsonSerializer.Serialize(new
        {
            email = notification.Email.Value,
            userId = notification.UserId.Value.ToString()
        });

        var auditLog = AuditLog.Create(
            notification.ApplicationId,
            AuditEventConstants.UserLoginSuccess,
            notification.Email.Value,
            notification.UserId.Value.ToString(),
            details,
            _currentUserService.IpAddress);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
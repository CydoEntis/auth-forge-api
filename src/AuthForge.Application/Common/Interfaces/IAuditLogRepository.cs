using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        ApplicationId? applicationId,
        string? eventType,
        string? targetUserId,
        DateTime? startDate,
        DateTime? endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
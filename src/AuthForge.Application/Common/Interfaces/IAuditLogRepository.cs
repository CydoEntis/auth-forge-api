using AuthForge.Application.AuditLogs.Enums;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Entities;
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
        AuditLogSortBy sortBy,
        SortOrder sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
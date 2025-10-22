using AuthForge.Application.Common.Extensions;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuthForgeDbContext _context;

    public AuditLogRepository(AuthForgeDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
    }


    // TODO: COme back and clean this up, move query and pagination out into own query builder.
    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        ApplicationId? applicationId,
        string? eventType,
        string? targetUserId,
        DateTime? startDate,
        DateTime? endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        query = query.WhereIf(
            applicationId != null,
            a => a.ApplicationId == applicationId);

        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(eventType),
            a => a.EventType.Contains(eventType!));

        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(targetUserId),
            a => a.TargetUserId == targetUserId);

        query = query.WhereIf(
            startDate.HasValue,
            a => a.Timestamp >= startDate!.Value);

        query = query.WhereIf(
            endDate.HasValue,
            a => a.Timestamp <= endDate!.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
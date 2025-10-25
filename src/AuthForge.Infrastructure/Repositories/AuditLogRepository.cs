using AuthForge.Application.AuditLogs.Enums;
using AuthForge.Application.Common.Extensions;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Entities;
using AuthForge.Infrastructure.Data;
using AuthForge.Infrastructure.Repositories.QueryBuilders;
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

    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        ApplicationId? applicationId,
        string? eventType,
        string? targetUserId,
        DateTime? startDate,
        DateTime? endDate,
        AuditLogSortBy sortBy,
        SortOrder sortOrder,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .AsQueryable()
            .ApplyFilters(applicationId, eventType, targetUserId, startDate, endDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplySorting(sortBy, sortOrder)
            .Paginate(pageNumber, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
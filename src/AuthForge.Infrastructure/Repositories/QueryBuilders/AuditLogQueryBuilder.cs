using AuthForge.Application.AuditLogs.Enums;
using AuthForge.Application.Common.Extensions;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Entities;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Infrastructure.Repositories.QueryBuilders;

public static class AuditLogQueryBuilder
{
    public static IQueryable<AuditLog> ApplyFilters(
        this IQueryable<AuditLog> query,
        ApplicationId? applicationId,
        string? eventType,
        string? targetUserId,
        DateTime? startDate,
        DateTime? endDate)
    {
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

        return query;
    }

    public static IQueryable<AuditLog> ApplySorting(
        this IQueryable<AuditLog> query,
        AuditLogSortBy sortBy,
        SortOrder sortOrder)
    {
        return sortBy switch
        {
            AuditLogSortBy.EventType => query.OrderByDirection(a => a.EventType, sortOrder),
            AuditLogSortBy.PerformedBy => query.OrderByDirection(a => a.PerformedBy, sortOrder),
            AuditLogSortBy.Timestamp or _ => query.OrderByDirection(a => a.Timestamp, sortOrder)
        };
    }
}
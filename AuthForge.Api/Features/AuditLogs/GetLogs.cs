using AuthForge.Api.Features.Shared.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace AuthForge.Api.Features.AuditLogs;

public record AuditLogRequest(
    string ApplicationId,
    string? EventType,
    string? TargetUserId,
    DateTime StartDate,
    DateTime EndDate);

public enum AuditLogSortBy
{
    Timestamp,
    EventType,
    PerformedBy
}

public record GetAuditLogsRequest(
    AuditLogRequest Request,
    PagedRequest Page,
    AuditLogSortBy SortBy,
    SortOrder SortOrder);

public class GetLogs
{
}
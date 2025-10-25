using AuthForge.Application.AuditLogs.Enums;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.AuditLogs.Queries;

public record GetAuditLogsQuery(
    ApplicationId ApplicationId,
    string? EventType,
    string? TargetUserId,
    DateTime? StartDate,
    DateTime? EndDate,
    int PageNumber,
    int PageSize,
    AuditLogSortBy SortBy,
    SortOrder SortOrder) : IQuery<Result<PagedResponse<AuditLogResponse>>>;
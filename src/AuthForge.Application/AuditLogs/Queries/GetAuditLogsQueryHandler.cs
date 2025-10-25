using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.AuditLogs.Queries;

public sealed class GetAuditLogsQueryHandler
    : IQueryHandler<GetAuditLogsQuery, Result<PagedResponse<AuditLogResponse>>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async ValueTask<Result<PagedResponse<AuditLogResponse>>> Handle(
        GetAuditLogsQuery query,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _auditLogRepository.GetPagedAsync(
            query.ApplicationId,
            query.EventType,
            query.TargetUserId,
            query.StartDate,
            query.EndDate,
            query.SortBy,
            query.SortOrder,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var responses = items.Select(log => new AuditLogResponse(
            log.Id.Value,
            log.ApplicationId.Value,
            log.EventType,
            log.PerformedBy,
            log.TargetUserId,
            log.Details,
            log.IpAddress,
            log.Timestamp)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var response = new PagedResponse<AuditLogResponse>
        {
            Items = responses,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };

        return Result<PagedResponse<AuditLogResponse>>.Success(response);
    }
}
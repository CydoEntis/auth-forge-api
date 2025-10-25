using AuthForge.Api.Common.Responses;
using AuthForge.Application.AuditLogs.Enums;
using AuthForge.Application.AuditLogs.Queries;
using AuthForge.Application.Common.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.AuditLogs;

public static class GetAuditLogsEndpoint
{
    public static IEndpointRouteBuilder MapGetAuditLogsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/audit-logs", Handle)
            .RequireAuthorization("Admin")
            .WithName("GetAuditLogs")
            .WithTags("Audit Logs")
            .WithDescription("Get paginated and filtered audit logs")
            .Produces<ApiResponse<PagedResponse<AuditLogResponse>>>(StatusCodes.Status200OK) 
            .Produces<
                ApiResponse<PagedResponse<AuditLogResponse>>>(StatusCodes.Status400BadRequest) 
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromQuery] Guid? applicationId,
        [FromQuery] string? eventType,
        [FromQuery] string? targetUserId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] AuditLogSortBy sortBy = AuditLogSortBy.Timestamp,
        [FromQuery] SortOrder sortOrder = SortOrder.Desc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromServices] IMediator mediator = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery(
            applicationId.HasValue ? ApplicationId.Create(applicationId.Value) : null,
            eventType,
            targetUserId,
            startDate,
            endDate,
            pageNumber,
            pageSize,
            sortBy,
            sortOrder
        );

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<PagedResponse<AuditLogResponse>>.FailureResponse(
                result.Error.Code,
                result.Error.Message);
            return Results.BadRequest(errorResponse);
        }

        var successResponse = ApiResponse<PagedResponse<AuditLogResponse>>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Models;
using AuthForge.Application.Applications.Queries.GetMy;
using AuthForge.Application.Common.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class GetMyApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapGetMyApplicationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications", Handle)
            .WithName("GetMyApplications")
            .WithTags("Applications")
            .WithDescription("Get all applications for the authenticated user with pagination, search, and sorting")
            .Produces<ApiResponse<PagedResponse<ApplicationSummary>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PagedResponse<ApplicationSummary>>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetMyApplicationsRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetMyApplicationsQuery
        {
            UserId = request.UserId,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10,
            SearchTerm = request.SearchTerm,
            SortBy = request.SortBy ?? ApplicationSortField.CreatedAt,
            SortOrder = request.SortOrder ?? SortOrder.Desc
        };

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<PagedResponse<ApplicationSummary>>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<PagedResponse<ApplicationSummary>>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record GetMyApplicationsRequest
{
    [FromQuery] public required string UserId { get; init; }
    [FromQuery] public int? PageNumber { get; init; }
    [FromQuery] public int? PageSize { get; init; }
    [FromQuery] public string? SearchTerm { get; init; }
    [FromQuery] public ApplicationSortField? SortBy { get; init; }
    [FromQuery] public SortOrder? SortOrder { get; init; }
}
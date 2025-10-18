using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Models;
using AuthForge.Application.Applications.Queries.GetAll;
using AuthForge.Application.Common.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class GetApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapGetApplicationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications", HandleAsync)
            .RequireAuthorization("Admin")
            .WithName("GetApplications")
            .WithTags("Applications")
            .WithDescription("Get paginated list of applications with filtering and sorting")
            .Produces<ApiResponse<PagedResponse<ApplicationSummary>>>(StatusCodes.Status200OK)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] ApplicationFilterParameters parameters,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApplicationsQuery(parameters);
        var result = await mediator.Send(query, cancellationToken);

        var successResponse = ApiResponse<PagedResponse<ApplicationSummary>>
            .SuccessResponse(result.Value);

        return Results.Ok(successResponse);
    }
}
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Models;
using AuthForge.Application.Applications.Queries.GetById;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class GetApplicationByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetApplicationByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications/{id}", Handle)
            .WithName("GetApplicationById")
            .WithTags("Applications")
            .WithDescription("Get application by ID")
            .Produces<ApiResponse<ApplicationDetail>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ApplicationDetail>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<ApplicationDetail>>(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromRoute] string id,
        [FromQuery] string userId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetApplicationByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<ApplicationDetail>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ApplicationDetail>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
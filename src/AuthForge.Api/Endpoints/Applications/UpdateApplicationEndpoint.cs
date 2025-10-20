using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.UpdateApplication;
using AuthForge.Application.Applications.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class UpdateApplicationEndpoint
{
    public static IEndpointRouteBuilder MapUpdateApplicationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/applications/{id}", Handle)
            .RequireAuthorization("Admin") 
            .WithName("UpdateApplication")
            .WithTags("Applications")
            .WithDescription("Update application name and settings")
            .Produces<ApiResponse<UpdateApplicationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateApplicationResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<UpdateApplicationResponse>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<UpdateApplicationResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromRoute] string id,
        [FromBody] UpdateApplicationRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateApplicationCommand(
            id,
            request.Name,
            request.Settings);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<UpdateApplicationResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<UpdateApplicationResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record UpdateApplicationRequest(
    string Name,
    AppSettings Settings);
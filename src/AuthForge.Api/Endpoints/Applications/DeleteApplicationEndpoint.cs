using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.DeleteApplication;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class DeleteApplicationEndpoint
{
    public static IEndpointRouteBuilder MapDeleteApplicationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/applications/{id}", Handle)
            .RequireAuthorization("Admin") 
            .WithName("DeleteApplication")
            .WithTags("Applications")
            .WithDescription("Delete (deactivate) an application")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<bool>>(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromRoute] string id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteApplicationCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<bool>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        return Results.NoContent();
    }
}
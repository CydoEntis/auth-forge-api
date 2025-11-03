using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.RegenerateJwtSecret;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class RegenerateJwtSecretEndpoint
{
    public static IEndpointRouteBuilder MapRegenerateJwtSecretEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/applications/{id}/regenerate-jwt-secret", Handle)
            .RequireAuthorization("Admin")
            .WithName("RegenerateJwtSecret")
            .WithTags("Applications")
            .WithDescription("Regenerate the JWT secret for an application. This will invalidate all existing JWTs.")
            .Produces<ApiResponse<RegenerateJwtSecretResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RegenerateJwtSecretResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<RegenerateJwtSecretResponse>>(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromRoute] string id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegenerateJwtSecretCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RegenerateJwtSecretResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<RegenerateJwtSecretResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
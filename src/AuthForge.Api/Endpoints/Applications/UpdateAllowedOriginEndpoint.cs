using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.AllowedOrigins.UpdateAllowedOrigin;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.Applications;

public static class UpdateAllowedOriginEndpoint
{
    public static IEndpointRouteBuilder MapUpdateAllowedOriginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/applications/{applicationId}/origins", Handle)
            .RequireAuthorization("Admin")
            .WithName("UpdateAllowedOrigin")
            .WithTags("Applications")
            .WithDescription("Update an allowed origin for an application")
            .Produces<ApiResponse<UpdateAllowedOriginResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateAllowedOriginResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateAllowedOriginResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string applicationId,
        [FromBody] UpdateAllowedOriginRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(applicationId, out var guid))
        {
            var errorResponse = ApiResponse<UpdateAllowedOriginResponse>.FailureResponse(
                ApplicationErrors.InvalidId.Code,
                ApplicationErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new UpdateAllowedOriginCommand(
            ApplicationId.Create(guid),
            request.OldOrigin,
            request.NewOrigin);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<UpdateAllowedOriginResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<UpdateAllowedOriginResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record UpdateAllowedOriginRequest(
    string OldOrigin,
    string NewOrigin);
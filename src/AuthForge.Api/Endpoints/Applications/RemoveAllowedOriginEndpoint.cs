using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.AllowedOrigins.RemoveAllowedOrigin;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.Applications;

public static class RemoveAllowedOriginEndpoint
{
    public static IEndpointRouteBuilder MapRemoveAllowedOriginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/applications/{applicationId}/origins", Handle)
            .RequireAuthorization("Admin")
            .WithName("RemoveAllowedOrigin")
            .WithTags("Applications")
            .WithDescription("Remove an allowed origin from an application")
            .Produces<ApiResponse<RemoveAllowedOriginResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RemoveAllowedOriginResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<RemoveAllowedOriginResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string applicationId,
        [FromBody] RemoveAllowedOriginRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(applicationId, out var guid))
        {
            var errorResponse = ApiResponse<RemoveAllowedOriginResponse>.FailureResponse(
                ApplicationErrors.InvalidId.Code,
                ApplicationErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new RemoveAllowedOriginCommand(
            ApplicationId.Create(guid),
            request.Origin);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RemoveAllowedOriginResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<RemoveAllowedOriginResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record RemoveAllowedOriginRequest(string Origin);
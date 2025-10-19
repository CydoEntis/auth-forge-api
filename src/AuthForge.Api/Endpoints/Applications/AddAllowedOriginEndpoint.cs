// src/AuthForge.Api/Endpoints/Applications/AddAllowedOriginEndpoint.cs

using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.AddAllowedOrigin;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Applications;

public static class AddAllowedOriginEndpoint
{
    public static IEndpointRouteBuilder MapAddAllowedOriginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/applications/{applicationId}/origins", HandleAddOrigin)
            .WithName("AddAllowedOrigin")
            .WithTags("Applications")
            .RequireAuthorization("AdminOnly")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleAddOrigin(
        [FromRoute] string applicationId,
        [FromBody] AddAllowedOriginRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AddAllowedOriginCommand(applicationId, request.Origin);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<object>.FailureResponse(
                result.Error.Code,
                result.Error.Message);
            
            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<object>.SuccessResponse(new { message = "Origin added successfully" });
        return Results.Ok(successResponse);
    }
}

public record AddAllowedOriginRequest(string Origin);
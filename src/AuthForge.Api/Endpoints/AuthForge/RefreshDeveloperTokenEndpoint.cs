using AuthForge.Api.Common.Responses;
using AuthForge.Api.Dtos.AuthForge;
using AuthForge.Application.AuthForge.Commands.Refresh;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.AuthForge;

public static class RefreshDeveloperTokenEndpoint
{
    public static IEndpointRouteBuilder MapRefreshDeveloperTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/authforge/refresh", HandleRefresh)
            .WithName("RefreshDeveloperToken")
            .WithTags("AuthForge")
            .WithDescription("Refresh developer access token using refresh token")
            .Produces<ApiResponse<RefreshDeveloperTokenResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RefreshDeveloperTokenResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleRefresh(
        [FromBody] RefreshDeveloperTokenRequest request,
        [FromServices] IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new RefreshDeveloperTokenCommand(
            request.RefreshToken,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RefreshDeveloperTokenResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var successResponse = ApiResponse<RefreshDeveloperTokenResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}


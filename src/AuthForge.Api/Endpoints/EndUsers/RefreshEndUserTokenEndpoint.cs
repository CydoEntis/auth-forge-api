using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.Refresh;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class RefreshEndUserTokenEndpoint
{
    public static IEndpointRouteBuilder MapRefreshEndUserTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/refresh", HandleRefresh)
            .WithName("RefreshEndUserToken")
            .WithTags("EndUsers")
            .WithDescription("Refresh end user access token using refresh token")
            .Produces<ApiResponse<RefreshEndUserTokenResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RefreshEndUserTokenResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleRefresh(
        [FromBody] RefreshEndUserTokenRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var application = httpContext.Items["Application"] as Domain.Entities.Application;

        if (application is null)
        {
            var errorResponse = ApiResponse<RefreshEndUserTokenResponse>.FailureResponse(EndUserErrors.InvalidApiKey);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new RefreshEndUserTokenCommand(
            request.RefreshToken,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RefreshEndUserTokenResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<RefreshEndUserTokenResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record RefreshEndUserTokenRequest(string RefreshToken);
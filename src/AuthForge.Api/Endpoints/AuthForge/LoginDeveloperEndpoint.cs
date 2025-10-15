using AuthForge.Api.Common.Responses;
using AuthForge.Api.Dtos.AuthForge;
using AuthForge.Application.AuthForge.Commands.Login;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.AuthForge;

public static class LoginDeveloperEndpoint
{
    public static IEndpointRouteBuilder MapLoginDeveloperEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/authforge/login", HandleLogin)
            .WithName("LoginDeveloper")
            .WithTags("AuthForge")
            .WithDescription("Authenticate developer and receive JWT tokens")
            .Produces<ApiResponse<LoginDeveloperResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LoginDeveloperResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleLogin(
        [FromBody] LoginDeveloperRequest request,
        [FromServices] IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new LoginDeveloperCommand(
            request.Email,
            request.Password,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<LoginDeveloperResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var successResponse = ApiResponse<LoginDeveloperResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}


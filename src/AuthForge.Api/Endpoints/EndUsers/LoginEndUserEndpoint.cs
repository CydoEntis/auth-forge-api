using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.Login;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class LoginEndUserEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/login", HandleLogin)
            .WithName("LoginEndUser")
            .WithTags("EndUsers")
            .WithDescription("Authenticate end user and receive JWT tokens")
            .Produces<ApiResponse<LoginEndUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LoginEndUserResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleLogin(
        [FromBody] LoginEndUserRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var application = httpContext.Items["Application"] as Domain.Entities.Application;

        if (application is null)
        {
            var errorResponse = ApiResponse<LoginEndUserResponse>.FailureResponse(EndUserErrors.InvalidApiKey);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }


        var command = new LoginEndUserCommand(
            application.Id.Value.ToString(),
            request.Email,
            request.Password,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers.UserAgent.ToString());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<LoginEndUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<LoginEndUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record LoginEndUserRequest(
    string ApplicationId,
    string Email,
    string Password);
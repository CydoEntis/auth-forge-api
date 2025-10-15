using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.Register;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class RegisterEndUserEndpoint
{
    public static IEndpointRouteBuilder MapRegisterEndUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/register", HandleRegister)
            .WithName("RegisterEndUser")
            .WithTags("EndUsers")
            .WithDescription("Register a new end user for an application")
            .Produces<ApiResponse<RegisterEndUserResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<RegisterEndUserResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleRegister(
        [FromBody] RegisterEndUserRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterEndUserCommand(
            request.ApplicationId,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RegisterEndUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            return Results.BadRequest(errorResponse);
        }

        var successResponse = ApiResponse<RegisterEndUserResponse>.SuccessResponse(result.Value);
        return Results.Created($"/api/endusers/{result.Value.UserId}", successResponse);
    }
}

public record RegisterEndUserRequest(
    string ApplicationId,
    string Email,
    string Password,
    string FirstName,
    string LastName);
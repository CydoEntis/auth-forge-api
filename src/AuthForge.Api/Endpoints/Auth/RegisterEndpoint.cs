using AuthForge.Api.Common.Responses;
using AuthForge.Application.Auth.Commands.Register;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Auth;

public static class RegisterEndpoint
{
    public static IEndpointRouteBuilder MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", HandleRegister)
            .WithName("Register")
            .WithTags("Authentication")
            .Produces<ApiResponse<RegisterUserResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<RegisterUserResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleRegister([FromBody] RegisterUserCommand command, IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse =
                ApiResponse<RegisterUserResponse>.FailureResponse(result.Error.Code, result.Error.Message);

            return Results.BadRequest(errorResponse);
        }

        var successResponse = ApiResponse<RegisterUserResponse>.SuccessResponse(result.Value);

        return Results.Created($"/api/users/{result.Value.UserId}", successResponse);
    }
}
using AuthForge.Api.Common.Responses;
using AuthForge.Api.Dtos.AuthForge;
using AuthForge.Application.AuthForge.Commands.Register;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.AuthForge;

public static class RegisterDeveloperEndpoint
{
    public static IEndpointRouteBuilder MapRegisterDeveloperEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/authforge/register", HandleRegister)
            .WithName("RegisterDeveloper")
            .WithTags("AuthForge")
            .WithDescription("Register a new developer account")
            .Produces<ApiResponse<RegisterDeveloperResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<RegisterDeveloperResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleRegister(
        [FromBody] RegisterDeveloperRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RegisterDeveloperCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RegisterDeveloperResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            return Results.BadRequest(errorResponse);
        }

        var successResponse = ApiResponse<RegisterDeveloperResponse>.SuccessResponse(result.Value);
        return Results.Created($"/api/authforge/users/{result.Value.UserId}", successResponse);
    }
}


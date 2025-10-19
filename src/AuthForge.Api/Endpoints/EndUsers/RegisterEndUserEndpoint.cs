using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.Register;
using AuthForge.Domain.Errors;
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
            .Produces<ApiResponse<RegisterEndUserResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleRegister(
        [FromBody] RegisterEndUserRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var application = httpContext.Items["Application"] as Domain.Entities.Application;

        if (application is null)
        {
            var errorResponse = ApiResponse<RegisterEndUserResponse>.FailureResponse(EndUserErrors.InvalidApiKey);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }
        
        
        var command = new RegisterEndUserCommand(
            application.Id.Value.ToString(),
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

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
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
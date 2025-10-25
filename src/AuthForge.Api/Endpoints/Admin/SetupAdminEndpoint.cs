using AuthForge.Api.Attributes;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Commands.SetUpAdmin;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Admin;

public static class SetupAdminEndpoint
{
    public static IEndpointRouteBuilder MapSetupAdminEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/setup", Handle)
            .AllowAnonymous()
            .WithMetadata(new RateLimitAttribute(3, 60))
            .WithName("SetupAdmin")
            .WithTags("Admins")
            .WithDescription("First-time setup: Create the admin account (can only be called once)")
            .Produces<ApiResponse<SetupAdminResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<SetupAdminResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<SetupAdminResponse>>(StatusCodes.Status409Conflict)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] SetupAdminRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new SetupAdminCommand(
            request.Email,
            request.Password);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<SetupAdminResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = result.Error.Code == "Admin.AlreadyExists"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<SetupAdminResponse>.SuccessResponse(result.Value);
        return Results.Created("/api/admin/setup", successResponse);
    }
}

public record SetupAdminRequest(
    string Email,
    string Password);
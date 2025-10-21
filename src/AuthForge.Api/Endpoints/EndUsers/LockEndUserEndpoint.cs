using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.LockEndUser;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class LockEndUserEndpoint
{
    public static IEndpointRouteBuilder MapLockEndUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/{userId}/lock", Handle)
            .RequireAuthorization("Admin")
            .WithName("LockEndUser")
            .WithTags("EndUsers - Admin Management")
            .WithDescription("Manually lock an end user account")
            .Produces<ApiResponse<LockEndUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LockEndUserResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<LockEndUserResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string userId,
        [FromBody] LockEndUserRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            var errorResponse = ApiResponse<LockEndUserResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new LockEndUserCommand(
            EndUserId.Create(userGuid),
            request.LockoutMinutes);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<LockEndUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<LockEndUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record LockEndUserRequest(int LockoutMinutes);
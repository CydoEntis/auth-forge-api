using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.UnlockEndUser;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class UnlockEndUserEndpoint
{
    public static IEndpointRouteBuilder MapUnlockEndUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/{userId}/unlock", Handle)
            .RequireAuthorization("Admin")
            .WithName("UnlockEndUser")
            .WithTags("EndUsers - Admin Management")
            .WithDescription("Unlock an end user account")
            .Produces<ApiResponse<UnlockEndUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UnlockEndUserResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UnlockEndUserResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string userId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            var errorResponse = ApiResponse<UnlockEndUserResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new UnlockEndUserCommand(EndUserId.Create(userGuid));

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<UnlockEndUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<UnlockEndUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
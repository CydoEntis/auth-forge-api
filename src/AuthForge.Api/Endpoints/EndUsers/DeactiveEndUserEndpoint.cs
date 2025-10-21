using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.DeactivateEndUser;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class DeactivateEndUserEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateEndUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/endusers/{userId}/deactivate", Handle)
            .RequireAuthorization("Admin")
            .WithName("DeactivateEndUser")
            .WithTags("EndUsers - Admin Management")
            .WithDescription("Deactivate an end user account")
            .Produces<ApiResponse<DeactivateEndUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DeactivateEndUserResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DeactivateEndUserResponse>>(StatusCodes.Status404NotFound)
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
            var errorResponse = ApiResponse<DeactivateEndUserResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new DeactivateEndUserCommand(EndUserId.Create(userGuid));

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<DeactivateEndUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<DeactivateEndUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
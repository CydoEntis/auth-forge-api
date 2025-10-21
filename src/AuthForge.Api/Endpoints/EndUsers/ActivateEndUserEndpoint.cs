using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.ActivateEndUser;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class ActivateEndUserEndpoint
{
    public static IEndpointRouteBuilder MapActivateEndUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/endusers/{userId}/activate", Handle)
            .RequireAuthorization("Admin")
            .WithName("ActivateEndUser")
            .WithTags("EndUsers - Admin Management")
            .WithDescription("Activate an end user account")
            .Produces<ApiResponse<ActivateEndUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ActivateEndUserResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ActivateEndUserResponse>>(StatusCodes.Status404NotFound)
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
            var errorResponse = ApiResponse<ActivateEndUserResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new ActivateEndUserCommand(EndUserId.Create(userGuid));

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<ActivateEndUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ActivateEndUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
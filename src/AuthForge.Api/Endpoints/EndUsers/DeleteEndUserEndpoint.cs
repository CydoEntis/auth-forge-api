using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.DeleteEndUser;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class DeleteEndUserEndpoint
{
    public static IEndpointRouteBuilder MapDeleteEndUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/endusers/{userId}", Handle)
            .RequireAuthorization("Admin")
            .WithName("DeleteEndUser")
            .WithTags("EndUsers - Admin Management")
            .WithDescription("Delete an end user account (hard delete)")
            .Produces<ApiResponse<DeleteEndUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<DeleteEndUserResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<DeleteEndUserResponse>>(StatusCodes.Status404NotFound)
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
            var errorResponse = ApiResponse<DeleteEndUserResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new DeleteEndUserCommand(EndUserId.Create(userGuid));

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<DeleteEndUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<DeleteEndUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
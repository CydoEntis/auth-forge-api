using System.Security.Claims;
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.UpdateCurrentUser;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class UpdateCurrentUserEndpoint
{
    public static IEndpointRouteBuilder MapUpdateCurrentUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/endusers/me", Handle)
            .RequireAuthorization("EndUser")
            .WithName("UpdateCurrentUser")
            .WithTags("EndUsers")
            .WithDescription("Update current authenticated end user profile")
            .Produces<ApiResponse<UpdateCurrentUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateCurrentUserResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateCurrentUserResponse>>(StatusCodes.Status401Unauthorized)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        [FromBody] UpdateCurrentUserRequest request,
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            var errorResponse = ApiResponse<UpdateCurrentUserResponse>.FailureResponse(
                EndUserErrors.Unauthorized.Code,
                EndUserErrors.Unauthorized.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var command = new UpdateCurrentUserCommand(
            EndUserId.Create(userId),
            request.FirstName,
            request.LastName);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<UpdateCurrentUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<UpdateCurrentUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record UpdateCurrentUserRequest(
    string FirstName,
    string LastName);
using System.Security.Claims;
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Queries.GetCurrentUser;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class GetCurrentUserEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/endusers/me", Handle)
            .RequireAuthorization("EndUser")
            .WithName("GetCurrentUser")
            .WithTags("EndUsers")
            .WithDescription("Get current authenticated end user profile")
            .Produces<ApiResponse<GetCurrentUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetCurrentUserResponse>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<GetCurrentUserResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            var errorResponse = ApiResponse<GetCurrentUserResponse>.FailureResponse(
                EndUserErrors.Unauthorized.Code,
                EndUserErrors.Unauthorized.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
        }

        var query = new GetCurrentUserQuery(EndUserId.Create(userId));

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<GetCurrentUserResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<GetCurrentUserResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Queries.GetApplicationUserById;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.Applications;

public static class GetApplicationUserByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetApplicationUserByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications/{applicationId}/users/{userId}", Handle)
            .RequireAuthorization("Admin")
            .WithName("GetApplicationUserById")
            .WithTags("Applications - User Management")
            .WithDescription("Get a specific user for an application")
            .Produces<ApiResponse<GetApplicationUserByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetApplicationUserByIdResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GetApplicationUserByIdResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string applicationId,
        string userId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(applicationId, out var appGuid))
        {
            var errorResponse = ApiResponse<GetApplicationUserByIdResponse>.FailureResponse(
                ApplicationErrors.InvalidId.Code,
                ApplicationErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            var errorResponse = ApiResponse<GetApplicationUserByIdResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var query = new GetApplicationUserByIdQuery(
            ApplicationId.Create(appGuid),
            EndUserId.Create(userGuid));

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<GetApplicationUserByIdResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<GetApplicationUserByIdResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
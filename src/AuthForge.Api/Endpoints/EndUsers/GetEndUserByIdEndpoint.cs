using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Queries.GetById;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class GetEndUserByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetEndUserByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications/{applicationId}/users/{userId}", HandleAsync)
            .RequireAuthorization("Admin")
            .WithName("GetEndUserById")
            .WithTags("EndUsers")
            .WithDescription("Get a specific user for an application")
            .Produces<ApiResponse<GetEndUserByIdResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetEndUserByIdResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<GetEndUserByIdResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        string applicationId,
        string userId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(applicationId, out var appGuid))
        {
            var errorResponse = ApiResponse<GetEndUserByIdResponse>.FailureResponse(
                ApplicationErrors.InvalidId.Code,
                ApplicationErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            var errorResponse = ApiResponse<GetEndUserByIdResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var query = new GetEndUserByIdQuery(
            ApplicationId.Create(appGuid),
            EndUserId.Create(userGuid));

        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<GetEndUserByIdResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<GetEndUserByIdResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
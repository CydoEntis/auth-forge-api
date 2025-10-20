using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Queries.GetApplicationKeys;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.Applications;

public static class GetApplicationKeysEndpoint
{
    public static IEndpointRouteBuilder MapGetApplicationKeysEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications/{applicationId}/keys", Handle)
            .RequireAuthorization("Admin")
            .WithName("GetApplicationKeys")
            .WithTags("Applications")
            .WithDescription("Get API keys for an application (secret key is masked)")
            .Produces<ApiResponse<GetApplicationKeysResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<GetApplicationKeysResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string applicationId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(applicationId, out var guid))
        {
            var errorResponse = ApiResponse<GetApplicationKeysResponse>.FailureResponse(ApplicationErrors.InvalidId);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var query = new GetApplicationKeysQuery(ApplicationId.Create(guid));
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<GetApplicationKeysResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<GetApplicationKeysResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
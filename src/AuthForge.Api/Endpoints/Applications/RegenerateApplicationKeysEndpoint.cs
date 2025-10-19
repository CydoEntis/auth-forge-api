using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.RegenerateKeys;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.Applications;

public static class RegenerateApplicationKeysEndpoint
{
    public static IEndpointRouteBuilder MapRegenerateApplicationKeysEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/applications/{applicationId}/keys/regenerate", Handle)
            .RequireAuthorization("Admin")
            .WithName("RegenerateApplicationKeys")
            .WithTags("Applications")
            .WithDescription("Regenerate API keys for an application (⚠️ Secret key shown only once)")
            .Produces<ApiResponse<RegenerateApplicationKeysResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RegenerateApplicationKeysResponse>>(StatusCodes.Status404NotFound)
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
            var errorResponse = ApiResponse<RegenerateApplicationKeysResponse>.FailureResponse(ApplicationErrors.InvalidId);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new RegenerateApplicationKeysCommand(ApplicationId.Create(guid));
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<RegenerateApplicationKeysResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<RegenerateApplicationKeysResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
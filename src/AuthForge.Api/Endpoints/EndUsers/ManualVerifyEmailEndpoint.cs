using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.EndUsers.Commands.ManualVerifyEmail;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.EndUsers;

public static class ManualVerifyEmailEndpoint
{
    public static IEndpointRouteBuilder MapManualVerifyEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/endusers/{userId}/verify-email", Handle)
            .RequireAuthorization("Admin")
            .WithName("ManualVerifyEmail")
            .WithTags("EndUsers - Admin Management")
            .WithDescription("Manually verify an end user's email address (bypass token requirement)")
            .Produces<ApiResponse<ManualVerifyEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ManualVerifyEmailResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ManualVerifyEmailResponse>>(StatusCodes.Status404NotFound)
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
            var errorResponse = ApiResponse<ManualVerifyEmailResponse>.FailureResponse(
                EndUserErrors.InvalidId.Code,
                EndUserErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new ManualVerifyEmailCommand(EndUserId.Create(userGuid));

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<ManualVerifyEmailResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ManualVerifyEmailResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}
using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.UpdateApplicationEmailSettings;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.Applications;

public static class UpdateApplicationEmailSettingsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/applications/{applicationId}/email", Handle)
            .RequireAuthorization("Admin")
            .WithName("UpdateEmail")
            .WithTags("Applications")
            .WithDescription("Update email settings for an application")
            .Produces<ApiResponse<UpdateApplicationEmailSettingsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UpdateApplicationEmailSettingsResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<UpdateApplicationEmailSettingsResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string applicationId,
        [FromBody] UpdateEmailRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(applicationId, out var guid))
        {
            var errorResponse = ApiResponse<UpdateApplicationEmailSettingsResponse>.FailureResponse(
                ApplicationErrors.InvalidId.Code,
                ApplicationErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new UpdateApplicationEmailSettingsCommand(
            ApplicationId.Create(guid),
            request.Provider,
            request.ApiKey,
            request.FromEmail,
            request.FromName,
            request.PasswordResetCallbackUrl,
            request.EmailVerificationCallbackUrl);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<UpdateApplicationEmailSettingsResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<UpdateApplicationEmailSettingsResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record UpdateEmailRequest(
    EmailProvider Provider,
    string ApiKey,
    string FromEmail,
    string FromName,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl);
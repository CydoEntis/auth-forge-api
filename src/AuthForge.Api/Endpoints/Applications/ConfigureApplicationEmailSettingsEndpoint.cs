using AuthForge.Api.Common.Mappings;
using AuthForge.Api.Common.Responses;
using AuthForge.Application.Applications.Commands.ConfigureApplicationEmailSettings;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Api.Endpoints.Applications;

public static class ConfigureApplicationEmailSettingsEndpoint
{
    public static IEndpointRouteBuilder MapConfigureEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/applications/{applicationId}/email", Handle)
            .RequireAuthorization("Admin")
            .WithName("ConfigureEmail")
            .WithTags("Applications")
            .WithDescription("Configure email settings for an application")
            .Produces<ApiResponse<ConfigureApplicationEmailSettingsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ConfigureApplicationEmailSettingsResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ConfigureApplicationEmailSettingsResponse>>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> Handle(
        string applicationId,
        [FromBody] ConfigureEmailRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(applicationId, out var guid))
        {
            var errorResponse = ApiResponse<ConfigureApplicationEmailSettingsResponse>.FailureResponse(
                ApplicationErrors.InvalidId.Code,
                ApplicationErrors.InvalidId.Message);
            return Results.Json(errorResponse, statusCode: StatusCodes.Status400BadRequest);
        }

        var command = new ConfigureApplicationEmailSettingsCommand(
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
            var errorResponse = ApiResponse<ConfigureApplicationEmailSettingsResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = ErrorMapper.ToStatusCode(result.Error);
            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<ConfigureApplicationEmailSettingsResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record ConfigureEmailRequest(
    EmailProvider Provider,
    string ApiKey,
    string FromEmail,
    string FromName,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl);
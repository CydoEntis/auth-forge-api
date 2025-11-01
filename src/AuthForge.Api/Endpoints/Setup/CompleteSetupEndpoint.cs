using AuthForge.Api.Common.Responses;
using AuthForge.Application.Setup.CompleteSetup;
using AuthForge.Domain.Enums;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using EmailProvider = AuthForge.Domain.Enums.EmailProvider;

namespace AuthForge.Api.Endpoints.Setup;

public static class CompleteSetupEndpoint
{
    public static void MapCompleteSetupEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/setup/complete", Handle)
            .AllowAnonymous()
            .WithName("CompleteSetup")
            .WithTags("Setup")
            .WithDescription("Complete the initial setup wizard")
            .Produces<ApiResponse<CompleteSetupResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<CompleteSetupResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<CompleteSetupResponse>>(StatusCodes.Status409Conflict)
            .WithOpenApi();
    }

    private static async Task<IResult> Handle(
        [FromBody] CompleteSetupRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CompleteSetupCommand(
            request.DatabaseType,
            request.ConnectionString,
            request.EmailProvider,
            request.ResendApiKey,
            request.SmtpHost,
            request.SmtpPort,
            request.SmtpUsername,
            request.SmtpPassword,
            request.SmtpUseSsl,
            request.FromEmail,
            request.FromName,
            request.AdminEmail,
            request.AdminPassword);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<CompleteSetupResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            var statusCode = result.Error.Code == "Setup.AlreadyComplete"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Results.Json(errorResponse, statusCode: statusCode);
        }

        var successResponse = ApiResponse<CompleteSetupResponse>.SuccessResponse(result.Value);
        return Results.Created("/api/setup/complete", successResponse);
    }
}

public record CompleteSetupRequest(
    DatabaseType DatabaseType,
    string? ConnectionString,
    EmailProvider EmailProvider,
    string? ResendApiKey,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    bool? SmtpUseSsl,
    string FromEmail,
    string FromName,
    string AdminEmail,
    string AdminPassword);
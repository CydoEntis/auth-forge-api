using AuthForge.Api.Common.Responses;
using AuthForge.Application.Setup.Commands.TestEmail;
using AuthForge.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AuthForge.Api.Endpoints.Setup;

public static class TestEmailEndpoint
{
    public static void MapTestEmailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/setup/test-email", Handle)
            .AllowAnonymous()
            .WithName("TestEmail")
            .WithTags("Setup")
            .WithDescription("Test email configuration before completing setup")
            .Produces<ApiResponse<TestEmailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<TestEmailResponse>>(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }

    private static async Task<IResult> Handle(
        [FromBody] TestEmailRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new TestEmailCommand(
            request.Provider,
            request.ResendApiKey,
            request.SmtpHost,
            request.SmtpPort,
            request.SmtpUsername,
            request.SmtpPassword,
            request.SmtpUseSsl,
            request.FromEmail,
            request.FromName,
            request.TestRecipient);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = ApiResponse<TestEmailResponse>.FailureResponse(
                result.Error.Code,
                result.Error.Message);

            return Results.BadRequest(errorResponse);
        }

        var successResponse = ApiResponse<TestEmailResponse>.SuccessResponse(result.Value);
        return Results.Ok(successResponse);
    }
}

public record TestEmailRequest(
    EmailProvider Provider,
    string? ResendApiKey,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    bool? SmtpUseSsl,
    string FromEmail,
    string FromName,
    string TestRecipient);
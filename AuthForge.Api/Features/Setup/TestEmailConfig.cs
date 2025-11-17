using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Features.Shared.Models;
using AuthForge.Api.Features.Shared.Validators;
using FluentValidation;

namespace AuthForge.Api.Features.Setup;

public class TestEmailConfigHandler
{
    private readonly IEmailTestService _emailTestService;
    private readonly ILogger<TestEmailConfigHandler> _logger;

    public TestEmailConfigHandler(
        IEmailTestService emailTestService,
        ILogger<TestEmailConfigHandler> logger)
    {
        _emailTestService = emailTestService;
        _logger = logger;
    }

    public async Task<TestEmailConfigResponse> HandleAsync(
        TestEmailConfigRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Testing email configuration during setup");

        if (!string.IsNullOrEmpty(request.SmtpHost))
        {
            await _emailTestService.TestSmtpAsync(
                request.SmtpHost,
                request.SmtpPort!.Value,
                request.SmtpUsername!,
                request.SmtpPassword!,
                request.UseSsl,
                request.FromEmail,
                request.FromName,
                request.TestRecipient,
                ct);
        }
        else if (!string.IsNullOrEmpty(request.ResendApiKey))
        {
            await _emailTestService.TestResendAsync(
                request.ResendApiKey,
                request.FromEmail,
                request.FromName,
                request.TestRecipient,
                ct);
        }
        else
        {
            throw new InvalidOperationException("No email provider configured");
        }

        return new TestEmailConfigResponse(true, "Test email sent successfully");
    }
}

public static class TestEmailConfigFeature
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/setup/test-email", async (
                TestEmailConfigRequest request,
                TestEmailConfigHandler handler,
                CancellationToken ct) =>
            {
                var validator = new TestEmailConfigValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<TestEmailConfigResponse>.Ok(response));
            })
            .WithName("TestEmailConfig")
            .WithTags("Setup")
            .AllowAnonymous();
    }
}
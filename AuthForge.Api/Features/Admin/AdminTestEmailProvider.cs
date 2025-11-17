using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Features.Shared.Models;
using AuthForge.Api.Features.Shared.Validators;
using FluentValidation;

namespace AuthForge.Api.Features.Admin;

public class AdminTestEmailProviderHandler
{
    private readonly IEmailTestService _emailTestService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AdminTestEmailProviderHandler> _logger;

    public AdminTestEmailProviderHandler(
        IEmailTestService emailTestService,
        ICurrentUserService currentUser,
        ILogger<AdminTestEmailProviderHandler> logger)
    {
        _emailTestService = emailTestService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<TestEmailConfigResponse> HandleAsync(
        TestEmailConfigRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Admin {UserId} testing email configuration", _currentUser.UserId);

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

public static class AdminTestEmailProvider
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/test-email", async (
                TestEmailConfigRequest request,
                AdminTestEmailProviderHandler handler,
                CancellationToken ct) =>
            {
                var validator = new TestEmailConfigValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<TestEmailConfigResponse>.Ok(response));
            })
            .WithName("AdminTestEmailProvider")
            .WithTags("Admin")
            .RequireAuthorization();
    }
}
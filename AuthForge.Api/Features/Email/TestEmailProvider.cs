using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using FluentValidation;

namespace AuthForge.Api.Features.Email;

public sealed record TestEmailProviderRequest
{
    public string FromEmail { get; init; } = null!;
    public string? FromName { get; init; }
    public string TestRecipient { get; init; } = null!;

    // SMTP Configuration
    public string? SmtpHost { get; init; }
    public int? SmtpPort { get; init; }
    public string? SmtpUsername { get; init; }
    public string? SmtpPassword { get; init; }
    public bool UseSsl { get; init; } = true;

    // Resend Configuration
    public string? ResendApiKey { get; init; }
}

public sealed record TestEmailProviderResponse(
    bool Success,
    string Message
);

public sealed class TestEmailConfigValidator : AbstractValidator<TestEmailProviderRequest>
{
    public TestEmailConfigValidator()
    {
        RuleFor(x => x.FromEmail)
            .NotEmpty().WithMessage("From email is required")
            .EmailAddress().WithMessage("From email must be a valid email address");

        RuleFor(x => x.TestRecipient)
            .NotEmpty().WithMessage("Test recipient email is required")
            .EmailAddress().WithMessage("Test recipient must be a valid email address");

        RuleFor(x => x)
            .Must(HaveEmailProviderConfigured)
            .WithMessage("Either SMTP or Resend configuration must be provided");

        // SMTP-specific validation
        When(x => !string.IsNullOrEmpty(x.SmtpHost), () =>
        {
            RuleFor(x => x.SmtpPort)
                .NotNull().WithMessage("SMTP port is required when SMTP host is provided")
                .InclusiveBetween(1, 65535).WithMessage("SMTP port must be between 1 and 65535");

            RuleFor(x => x.SmtpUsername)
                .NotEmpty().WithMessage("SMTP username is required when SMTP host is provided");

            RuleFor(x => x.SmtpPassword)
                .NotEmpty().WithMessage("SMTP password is required when SMTP host is provided");
        });

        // Resend-specific validation
        When(x => !string.IsNullOrEmpty(x.ResendApiKey), () =>
        {
            RuleFor(x => x.ResendApiKey)
                .NotEmpty().WithMessage("Resend API key is required");
        });
    }

    private static bool HaveEmailProviderConfigured(TestEmailProviderRequest request)
    {
        var hasSmtp = !string.IsNullOrEmpty(request.SmtpHost);
        var hasResend = !string.IsNullOrEmpty(request.ResendApiKey);
        return hasSmtp || hasResend;
    }
}

public class TestEmailProviderHandler
{
    private readonly IEmailTestService _emailTestService;
    private readonly ILogger<TestEmailProviderHandler> _logger;

    public TestEmailProviderHandler(
        IEmailTestService emailTestService,
        ILogger<TestEmailProviderHandler> logger)
    {
        _emailTestService = emailTestService;
        _logger = logger;
    }

    public async Task<TestEmailProviderResponse> HandleAsync(
        TestEmailProviderRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Testing email configuration");

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

            return new TestEmailProviderResponse(true, "SMTP test email sent successfully");
        }

        if (!string.IsNullOrEmpty(request.ResendApiKey))
        {
            await _emailTestService.TestResendAsync(
                request.ResendApiKey,
                request.FromEmail,
                request.FromName,
                request.TestRecipient,
                ct);

            return new TestEmailProviderResponse(true, "Resend test email sent successfully");
        }

        throw new InvalidOperationException("No email provider configured");
    }
}

public static class TestEmailProvider
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/test", async (
                TestEmailProviderRequest request,
                TestEmailProviderHandler handler,
                CancellationToken ct) =>
            {
                var validator = new TestEmailConfigValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<TestEmailProviderResponse>.Ok(response));
            })
            .WithName("TestEmailConfig")
            .WithTags("Email")
            .AllowAnonymous();
    }
}
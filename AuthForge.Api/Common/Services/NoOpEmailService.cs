namespace AuthForge.Api.Common.Services;

using AuthForge.Providers.Interfaces;

public class NoOpEmailService : IEmailService
{
    private readonly ILogger _logger;

    public NoOpEmailService(ILogger logger)
    {
        _logger = logger;
    }

    public Task<EmailResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        _logger.LogWarning("Email service not configured. Cannot send email to {To}", message.To);
        
        return Task.FromResult(new EmailResult(
            Success: false,
            ErrorMessage: "Email service not configured. Please complete setup."));
    }

    public Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        return Task.FromResult(false);
    }
}
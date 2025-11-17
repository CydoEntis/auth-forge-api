namespace AuthForge.Providers.Interfaces;

public interface IEmailService
{
    Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);

    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

public record EmailMessage(
    string To,
    string Subject,
    string Body,
    string? From = null,
    string? FromName = null,
    bool IsHtml = true
);

public record EmailResult(
    bool Success,
    string? MessageId = null,
    string? ErrorMessage = null
);
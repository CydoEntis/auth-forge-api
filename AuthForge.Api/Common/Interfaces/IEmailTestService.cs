namespace AuthForge.Api.Common.Interfaces;

public interface IEmailTestService
{
    Task TestSmtpAsync(
        string smtpHost,
        int smtpPort,
        string smtpUsername,
        string smtpPassword,
        bool useSsl,
        string fromEmail,
        string? fromName,
        string testRecipient,
        CancellationToken ct);

    Task TestResendAsync(
        string resendApiKey,
        string fromEmail,
        string? fromName,
        string testRecipient,
        CancellationToken ct);
}
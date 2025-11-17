using AuthForge.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Resend;
using EmailMessage = AuthForge.Providers.Interfaces.EmailMessage;
using ResendEmailMessage = Resend.EmailMessage;

namespace AuthForge.Providers.Email;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend resend,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _logger = logger;
    }

    public async Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resendMessage = new ResendEmailMessage();
            resendMessage.From = string.IsNullOrWhiteSpace(message.FromName)
                ? message.From!
                : $"{message.FromName} <{message.From}>";
            resendMessage.To.Add(message.To);
            resendMessage.Subject = message.Subject;
            resendMessage.HtmlBody = message.IsHtml ? message.Body : null;
            resendMessage.TextBody = !message.IsHtml ? message.Body : null;

            await _resend.EmailSendAsync(resendMessage);

            _logger.LogInformation(
                "Email sent via Resend to {To}",
                message.To);

            return new EmailResult(
                Success: true,
                MessageId: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Resend to {To}", message.To);
            return new EmailResult(
                Success: false,
                ErrorMessage: ex.Message);
        }
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return Task.FromResult(_resend != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resend connection test failed");
            return Task.FromResult(false);
        }
    }
}
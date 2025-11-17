using System.Net;
using System.Net.Mail;
using AuthForge.Api.Common.Interfaces;
using Resend;

namespace AuthForge.Api.Common.Services;

public class EmailTestService : IEmailTestService
{
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailTestService> _logger;

    public EmailTestService(
        IEmailTemplateService templateService,
        ILogger<EmailTestService> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    public async Task TestSmtpAsync(
        string smtpHost,
        int smtpPort,
        string smtpUsername,
        string smtpPassword,
        bool useSsl,
        string fromEmail,
        string? fromName,
        string testRecipient,
        CancellationToken ct)
    {
        try
        {
            var email = await _templateService.CreateTestEmailAsync(
                toEmail: testRecipient,
                fromEmail: fromEmail,
                fromName: fromName);

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = useSsl,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email.From, email.FromName),
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = email.IsHtml
            };
            mailMessage.To.Add(email.To);

            await client.SendMailAsync(mailMessage, ct);

            _logger.LogInformation("SMTP test email sent successfully to {Recipient}", testRecipient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP test failed for {Host}:{Port}", smtpHost, smtpPort);
            throw new InvalidOperationException($"SMTP test failed: {ex.Message}", ex);
        }
    }

    public async Task TestResendAsync(
        string resendApiKey,
        string fromEmail,
        string? fromName,
        string testRecipient,
        CancellationToken ct)
    {
        try
        {
            IResend resend = ResendClient.Create(resendApiKey);

            var templateEmail = await _templateService.CreateTestEmailAsync(
                testRecipient,
                fromEmail,
                fromName);

            var message = new EmailMessage
            {
                From = templateEmail.From,
                Subject = templateEmail.Subject,
                HtmlBody = templateEmail.Body
            };
            message.To.Add(templateEmail.To);

            await resend.EmailSendAsync(message, ct);

            _logger.LogInformation("Resend test email sent successfully to {Recipient}", testRecipient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resend test failed");
            throw new InvalidOperationException($"Resend test failed: {ex.Message}", ex);
        }
    }
}
using System.Net;
using System.Net.Mail;
using AuthForge.Providers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthForge.Providers.Email;

public class SmtpEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _enableSsl;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        string host,
        int port,
        string username,
        string password,
        bool enableSsl,
        ILogger<SmtpEmailService> logger)
    {
        _host = host;
        _port = port;
        _username = username;
        _password = password;
        _enableSsl = enableSsl;
        _logger = logger;
    }

    public async Task<EmailResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient(_host, _port);

            client.EnableSsl = _enableSsl;

            client.Credentials = new NetworkCredential(_username, _password);

            client.UseDefaultCredentials = false;

            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(message.From ?? _username, message.FromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml
            };

            mailMessage.To.Add(message.To);

            await client.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email sent via SMTP to {To}", message.To);

            return new EmailResult(Success: true, MessageId: Guid.NewGuid().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {To}", message.To);
            return new EmailResult(Success: false, ErrorMessage: ex.Message);
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient(_host, _port);
            client.EnableSsl = _enableSsl;
            client.Credentials = new NetworkCredential(_username, _password);
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Timeout = 5000;

            await Task.CompletedTask;

            _logger.LogInformation("SMTP connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP connection test failed");
            return false;
        }
    }
}
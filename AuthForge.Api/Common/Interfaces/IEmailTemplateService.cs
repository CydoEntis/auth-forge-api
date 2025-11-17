using AuthForge.Providers.Interfaces;

namespace AuthForge.Api.Common.Interfaces;

public interface IEmailTemplateService
{
    Task<EmailMessage> CreatePasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetUrl,
        string appName);

    Task<EmailMessage> CreateEmailVerificationEmailAsync(
        string toEmail,
        string toName,
        string verificationUrl,
        string appName);

    Task<EmailMessage> CreateTestEmailAsync(
        string toEmail,
        string fromEmail,
        string? fromName = null);
}
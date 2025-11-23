using System.Reflection;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Providers.Interfaces;

namespace AuthForge.Api.Common.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private static readonly string PasswordResetTemplate;
    private static readonly string EmailVerificationTemplate;
    private static readonly string TestEmailTemplate;
    private static readonly string AccountAlreadyExistsTemplate;

    static EmailTemplateService()
    {
        PasswordResetTemplate = LoadEmbeddedTemplate("DefaultPasswordResetTemplate.html");
        EmailVerificationTemplate = LoadEmbeddedTemplate("DefaultEmailVerificationTemplate.html");
        TestEmailTemplate = LoadEmbeddedTemplate("TestEmailTemplate.html");
        AccountAlreadyExistsTemplate = LoadEmbeddedTemplate("DefaultEmailAlreadyExistsTemplate.html");
    }

    public Task<EmailMessage> CreatePasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetUrl,
        string appName)
    {
        var html = PasswordResetTemplate
            .Replace("{{userName}}", toName)
            .Replace("{{resetUrl}}", resetUrl)
            .Replace("{{appName}}", appName);

        var message = new EmailMessage(
            To: toEmail,
            Subject: $"Reset your {appName} password",
            Body: html,
            From: null,
            FromName: appName,
            IsHtml: true
        );

        return Task.FromResult(message);
    }

    public Task<EmailMessage> CreateEmailVerificationEmailAsync(
        string toEmail,
        string toName,
        string verificationUrl,
        string appName)
    {
        var html = EmailVerificationTemplate
            .Replace("{{userName}}", toName)
            .Replace("{{verificationUrl}}", verificationUrl)
            .Replace("{{appName}}", appName);

        var message = new EmailMessage(
            To: toEmail,
            Subject: $"Welcome to {appName}! Verify your email",
            Body: html,
            From: null,
            FromName: appName,
            IsHtml: true
        );

        return Task.FromResult(message);
    }

    public Task<EmailMessage> CreateTestEmailAsync(
        string toEmail,
        string fromEmail,
        string? fromName = null)
    {
        var html = TestEmailTemplate
            .Replace("{{testDate}}", DateTime.UtcNow.ToString("f"));

        var message = new EmailMessage(
            To: toEmail,
            Subject: "AuthForge Email Configuration Test",
            Body: html,
            From: fromEmail,
            FromName: fromName ?? "AuthForge",
            IsHtml: true
        );

        return Task.FromResult(message);
    }

    public Task<EmailMessage> CreateAccountAlreadyExistsEmailAsync(
        string toEmail,
        string toName,
        string appName)
    {
        var html = AccountAlreadyExistsTemplate
            .Replace("{{userName}}", toName)
            .Replace("{{appName}}", appName);

        var message = new EmailMessage(
            To: toEmail,
            Subject: $"Account Registration Attempt - {appName}",
            Body: html,
            From: null,
            FromName: appName,
            IsHtml: true
        );

        return Task.FromResult(message);
    }

    private static string LoadEmbeddedTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"AuthForge.Api.Common.Templates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Template '{templateName}' not found");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
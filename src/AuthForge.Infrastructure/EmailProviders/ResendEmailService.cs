using System.Net.Http.Json;
using System.Reflection;
using AuthForge.Application.Common.Interfaces;

namespace AuthForge.Infrastructure.EmailProviders;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string? _passwordResetCallbackUrl;
    private readonly string? _emailVerificationCallbackUrl;

    private static readonly string PasswordResetTemplate;
    private static readonly string EmailVerificationTemplate;

    static ResendEmailService()
    {
        PasswordResetTemplate = LoadEmbeddedTemplate("DefaultPasswordResetTemplate.html");
        EmailVerificationTemplate = LoadEmbeddedTemplate("DefaultEmailVerificationTemplate.html");
    }

    public ResendEmailService(
        HttpClient httpClient,
        string apiKey,
        string fromEmail,
        string fromName,
        string? passwordResetCallbackUrl,
        string? emailVerificationCallbackUrl)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _fromEmail = fromEmail;
        _fromName = fromName;
        _passwordResetCallbackUrl = passwordResetCallbackUrl;
        _emailVerificationCallbackUrl = emailVerificationCallbackUrl;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetToken,
        string appName,
        CancellationToken cancellationToken = default)
    {
        var resetUrl = string.IsNullOrWhiteSpace(_passwordResetCallbackUrl)
            ? $"https://yourapp.com/reset-password?token={resetToken}"
            : _passwordResetCallbackUrl.Contains("{token}")
                ? _passwordResetCallbackUrl.Replace("{token}", resetToken)
                : $"{_passwordResetCallbackUrl}?token={resetToken}";

        var html = PasswordResetTemplate
            .Replace("{{userName}}", toName)
            .Replace("{{resetToken}}", resetToken)
            .Replace("{{resetUrl}}", resetUrl)
            .Replace("{{appName}}", appName);

        var request = new
        {
            from = $"{_fromName} <{_fromEmail}>",
            to = new[] { toEmail },
            subject = $"Reset your {appName} password",
            html
        };

        await SendEmailAsync(request, cancellationToken);
    }

    public async Task SendEmailVerificationEmailAsync(
        string toEmail,
        string toName,
        string verificationToken,
        string appName,
        CancellationToken cancellationToken = default)
    {
        var verificationUrl = string.IsNullOrWhiteSpace(_emailVerificationCallbackUrl)
            ? $"https://yourapp.com/verify?token={verificationToken}"
            : _emailVerificationCallbackUrl.Contains("{token}")
                ? _emailVerificationCallbackUrl.Replace("{token}", verificationToken)
                : $"{_emailVerificationCallbackUrl}?token={verificationToken}";

        var html = EmailVerificationTemplate
            .Replace("{{userName}}", toName)
            .Replace("{{verificationToken}}", verificationToken)
            .Replace("{{verificationUrl}}", verificationUrl)
            .Replace("{{appName}}", appName);

        var request = new
        {
            from = $"{_fromName} <{_fromEmail}>",
            to = new[] { toEmail },
            subject = $"Welcome to {appName}! Verify your email",
            html
        };

        await SendEmailAsync(request, cancellationToken);
    }

    private async Task SendEmailAsync(object request, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.resend.com/emails",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static string LoadEmbeddedTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"AuthForge.Infrastructure.EmailProviders.Templates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException(
                $"Email template '{templateName}' not found as embedded resource. " +
                $"Make sure the file is marked as an EmbeddedResource in the .csproj file.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
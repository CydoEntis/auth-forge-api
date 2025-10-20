namespace AuthForge.Domain.ValueObjects;

public sealed record ApplicationEmailSettings
{
    public EmailProvider Provider { get; private set; }
    public string ApiKey { get; private set; } = string.Empty;
    public string FromEmail { get; private set; } = string.Empty;
    public string FromName { get; private set; } = string.Empty;
    public string? PasswordResetCallbackUrl { get; private set; } // ✅ NEW
    public string? EmailVerificationCallbackUrl { get; private set; } // ✅ NEW

    private ApplicationEmailSettings()
    {
    }

    private ApplicationEmailSettings(
        EmailProvider provider,
        string apiKey,
        string fromEmail,
        string fromName,
        string? passwordResetCallbackUrl,
        string? emailVerificationCallbackUrl)
    {
        Provider = provider;
        ApiKey = apiKey;
        FromEmail = fromEmail;
        FromName = fromName;
        PasswordResetCallbackUrl = passwordResetCallbackUrl;
        EmailVerificationCallbackUrl = emailVerificationCallbackUrl;
    }

    public static ApplicationEmailSettings Create(
        EmailProvider provider,
        string apiKey,
        string fromEmail,
        string fromName,
        string? passwordResetCallbackUrl = null,
        string? emailVerificationCallbackUrl = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty", nameof(apiKey));

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new ArgumentException("From email cannot be empty", nameof(fromEmail));

        if (string.IsNullOrWhiteSpace(fromName))
            throw new ArgumentException("From name cannot be empty", nameof(fromName));

        if (!string.IsNullOrWhiteSpace(passwordResetCallbackUrl) &&
            !Uri.TryCreate(passwordResetCallbackUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid password reset callback URL", nameof(passwordResetCallbackUrl));

        if (!string.IsNullOrWhiteSpace(emailVerificationCallbackUrl) &&
            !Uri.TryCreate(emailVerificationCallbackUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid email verification callback URL",
                nameof(emailVerificationCallbackUrl));

        return new ApplicationEmailSettings(
            provider,
            apiKey,
            fromEmail,
            fromName,
            passwordResetCallbackUrl,
            emailVerificationCallbackUrl);
    }
}

public enum EmailProvider
{
    Resend = 1
}
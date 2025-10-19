namespace AuthForge.Domain.ValueObjects;

public sealed record ApplicationEmailSettings
{
    public EmailProvider Provider { get; private set; }
    public string ApiKey { get; private set; } = string.Empty;
    public string FromEmail { get; private set; } = string.Empty;
    public string FromName { get; private set; } = string.Empty;

    private ApplicationEmailSettings() { }

    private ApplicationEmailSettings(
        EmailProvider provider,
        string apiKey,
        string fromEmail,
        string fromName)
    {
        Provider = provider;
        ApiKey = apiKey;
        FromEmail = fromEmail;
        FromName = fromName;
    }

    public static ApplicationEmailSettings Create(
        EmailProvider provider,
        string apiKey,
        string fromEmail,
        string fromName)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be empty", nameof(apiKey));

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new ArgumentException("From email cannot be empty", nameof(fromEmail));

        if (string.IsNullOrWhiteSpace(fromName))
            throw new ArgumentException("From name cannot be empty", nameof(fromName));

        return new ApplicationEmailSettings(provider, apiKey, fromEmail, fromName);
    }
}

public enum EmailProvider
{
    Resend = 1
}
namespace AuthForge.Api.Entities;

public class ApplicationEmailSettings
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }

    public bool UseGlobalSettings { get; set; } = true;
    public string? Provider { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }

    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }
    public bool SmtpUseSsl { get; set; } = true;

    public string? ResendApiKeyEncrypted { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Application Application { get; set; } = null!;
}
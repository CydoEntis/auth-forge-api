namespace AuthForge.Api.Entities;

public class Configuration
{
    public int Id { get; set; }
    public bool IsSetupComplete { get; set; } = false;

    public string? AuthForgeDomain { get; set; } = string.Empty;
    public string? DatabaseProvider { get; set; }
    public string? DatabaseConnectionString { get; set; }

    public string? EmailProvider { get; set; }
    public string? EmailFromAddress { get; set; }
    public string? EmailFromName { get; set; }

    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordEncrypted { get; set; } // ALWAYS Store encrypted!
    public bool SmtpUseSsl { get; set; }

    public string? ResendApiKeyEncrypted { get; set; } // ALWAYS Store encrypted!

    public string? JwtSecretEncrypted { get; set; } // ALWAYS Store encrypted!

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
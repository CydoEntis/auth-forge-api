
using AuthForge.Api.Features.Shared.Enums;

namespace AuthForge.Api.Features.Shared.Models;

public record EmailProviderConfig
{
    public EmailProvider EmailProvider { get; init; }
    public string? FromEmail { get; init; }
    public string? FromName { get; init; }

    // SMTP
    public string? SmtpHost { get; init; }
    public int? SmtpPort { get; init; }
    public string? SmtpUsername { get; init; }
    public string? SmtpPassword { get; init; }
    public bool UseSsl { get; init; } = true;

    // Resend
    public string? ResendApiKey { get; init; }
}
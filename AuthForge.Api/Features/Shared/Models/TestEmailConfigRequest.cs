namespace AuthForge.Api.Features.Shared.Models;

public record TestEmailConfigRequest
{
    public string FromEmail { get; init; } = null!;
    public string? FromName { get; init; }
    public string TestRecipient { get; init; } = null!;

    public string? SmtpHost { get; init; }
    public int? SmtpPort { get; init; }
    public string? SmtpUsername { get; init; }
    public string? SmtpPassword { get; init; }
    public bool UseSsl { get; init; } = true;

    public string? ResendApiKey { get; init; }
}
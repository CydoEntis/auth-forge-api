using AuthForge.Domain.Enums;

namespace AuthForge.Application.Common.Models;

public record EmailConfiguration(
    EmailProvider Provider,
    string? ResendApiKey,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    bool? SmtpUseSsl,
    string FromEmail,
    string FromName);
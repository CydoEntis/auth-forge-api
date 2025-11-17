using AuthForge.Api.Features.Setup.Shared.Enums;
using AuthForge.Api.Features.Shared.Enums;

namespace AuthForge.Api.Features.Applications.Shared.Models;

public record EmailConfigRequest(
    EmailProvider EmailProvider,
    string? ResendApiKey,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    string FromEmail,
    string FromName,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl);
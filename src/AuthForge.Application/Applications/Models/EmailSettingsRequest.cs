using AuthForge.Domain.Enums;

namespace AuthForge.Application.Applications.Models;

public record EmailSettingsRequest(
    EmailProvider Provider,
    string ApiKey,
    string FromEmail,
    string FromName,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl);
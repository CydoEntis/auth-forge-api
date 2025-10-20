using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.UpdateEmail;

public record UpdateEmailCommand(
    ApplicationId ApplicationId,
    EmailProvider Provider,
    string ApiKey,
    string FromEmail,
    string FromName,
    string? PasswordResetCallbackUrl,
    string? EmailVerificationCallbackUrl) : ICommand<Result<UpdateEmailResponse>>;
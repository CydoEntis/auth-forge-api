using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.EndUsers.Commands.VerifyEmail;

public record VerifyEmailCommand(
    ApplicationId ApplicationId,
    Email Email,
    string VerificationToken) : ICommand<Result<VerifyEmailResponse>>;
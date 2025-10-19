using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.EndUsers.Commands.ForgotPassword;

public record ForgotPasswordCommand(
    ApplicationId ApplicationId,
    Email Email) : ICommand<Result<ForgotPasswordResponse>>;
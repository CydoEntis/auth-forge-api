using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.EndUsers.Commands.ResetPassword;

public record ResetPasswordCommand(
    ApplicationId ApplicationId,
    Email Email,
    string ResetToken,
    string NewPassword) : ICommand<Result<ResetPasswordResponse>>;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.ResetPassword;

public record ResetAdminPasswordCommand(
    string ResetToken,
    string NewPassword,
    string ConfirmPassword) : ICommand<Result<ResetAdminPasswordResponse>>;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.ChangePassword;

public sealed record ChangeAdminPasswordCommand(
    string CurrentPassword,
    string NewPassword) : ICommand<Result<ChangeAdminPasswordResponse>>;
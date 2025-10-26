using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.RequestPasswordReset;

public record RequestAdminPasswordResetCommand(
    string Email) : ICommand<Result<RequestAdminPasswordResetResponse>>;
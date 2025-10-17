using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands;

public sealed record LoginAdminCommand(
    string Email,
    string Password
) : ICommand<Result<LoginAdminResponse>>;
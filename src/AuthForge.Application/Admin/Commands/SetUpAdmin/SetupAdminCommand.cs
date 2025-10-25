using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.SetUpAdmin;

public record SetupAdminCommand(
    string Email,
    string Password) : ICommand<Result<SetupAdminResponse>>;
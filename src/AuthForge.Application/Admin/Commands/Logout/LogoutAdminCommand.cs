using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.Logout;

public sealed record LogoutAdminCommand : ICommand<Result>;
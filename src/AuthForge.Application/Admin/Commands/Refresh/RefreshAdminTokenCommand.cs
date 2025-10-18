using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.Refresh;

public sealed record RefreshAdminTokenCommand(
    string RefreshToken
) : ICommand<Result<RefreshAdminTokenResponse>>;
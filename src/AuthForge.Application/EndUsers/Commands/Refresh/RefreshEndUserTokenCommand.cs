using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.Refresh;

public sealed record RefreshEndUserTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<Result<RefreshEndUserTokenResponse>>;
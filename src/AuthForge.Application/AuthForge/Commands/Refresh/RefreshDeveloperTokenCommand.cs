using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.AuthForge.Commands.Refresh;

public sealed record RefreshDeveloperTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<Result<RefreshDeveloperTokenResponse>>;
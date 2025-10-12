using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Auth.Commands.Refresh;

public sealed class RefreshTokenCommand : ICommand<Result<RefreshTokenResponse>>
{
    public required string RefreshToken { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
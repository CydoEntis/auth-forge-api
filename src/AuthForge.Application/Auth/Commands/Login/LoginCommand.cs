using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Auth.Commands.Login;

public class LoginCommand : ICommand<Result<LoginResponse>>
{
    public required string TenantId { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? IpAddress {get; init; }
    public string? UserAgent {get; init; }
}
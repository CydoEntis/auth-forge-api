using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.Login;

public sealed record LoginEndUserCommand(
    string ApplicationId,
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<Result<LoginEndUserResponse>>;
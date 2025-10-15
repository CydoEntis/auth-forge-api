using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.AuthForge.Commands.Login;

public sealed record LoginDeveloperCommand(
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<Result<LoginDeveloperResponse>>;
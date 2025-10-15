using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.AuthForge.Commands.Register;

public sealed record RegisterDeveloperCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<Result<RegisterDeveloperResponse>>;
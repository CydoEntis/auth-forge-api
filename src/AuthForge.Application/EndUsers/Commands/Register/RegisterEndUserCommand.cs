using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.Register;

public sealed record RegisterEndUserCommand(
    string ApplicationId,
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<Result<RegisterEndUserResponse>>;
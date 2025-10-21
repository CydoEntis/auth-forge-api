using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.DeactivateEndUser;

public record DeactivateEndUserCommand(
    EndUserId UserId) : ICommand<Result<DeactivateEndUserResponse>>;


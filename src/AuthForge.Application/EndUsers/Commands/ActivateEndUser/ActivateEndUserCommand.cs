using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ActivateEndUser;

public record ActivateEndUserCommand(
    EndUserId UserId) : ICommand<Result<ActivateEndUserResponse>>;


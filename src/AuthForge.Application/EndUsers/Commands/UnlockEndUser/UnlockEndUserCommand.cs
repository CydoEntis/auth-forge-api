using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.UnlockEndUser;

public record UnlockEndUserCommand(
    EndUserId UserId) : ICommand<Result<UnlockEndUserResponse>>;


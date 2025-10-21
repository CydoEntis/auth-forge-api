using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.LockEndUser;

public record LockEndUserCommand(
    EndUserId UserId,
    int LockoutMinutes) : ICommand<Result<LockEndUserResponse>>;


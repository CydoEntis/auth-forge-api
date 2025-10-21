using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.DeleteEndUser;

public record DeleteEndUserCommand(
    EndUserId UserId) : ICommand<Result<DeleteEndUserResponse>>;


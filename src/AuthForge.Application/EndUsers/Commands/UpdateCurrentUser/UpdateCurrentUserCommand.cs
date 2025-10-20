using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand(
    EndUserId UserId,
    string FirstName,
    string LastName) : ICommand<Result<UpdateCurrentUserResponse>>;


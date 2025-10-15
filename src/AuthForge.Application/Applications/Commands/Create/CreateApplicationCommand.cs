using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.Create;

public sealed record CreateApplicationCommand(
    string UserId,
    string Name) : ICommand<Result<CreateApplicationResponse>>;
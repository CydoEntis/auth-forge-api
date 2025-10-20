using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.CreateApplication;

public sealed record CreateApplicationCommand(
    string Name) : ICommand<Result<CreateApplicationResponse>>;
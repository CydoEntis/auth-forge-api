using AuthForge.Application.Applications.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.Update;

public sealed record UpdateApplicationCommand(
    string ApplicationId,
    string UserId,
    string Name,
    AppSettings Settings) : ICommand<Result<UpdateApplicationResponse>>;
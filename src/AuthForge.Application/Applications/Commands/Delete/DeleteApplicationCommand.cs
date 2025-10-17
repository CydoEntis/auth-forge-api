using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.Delete;

public sealed record DeleteApplicationCommand(
    string ApplicationId) : ICommand<Result>;
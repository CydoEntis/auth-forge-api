using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.DeleteApplication;

public sealed record DeleteApplicationCommand(
    string ApplicationId) : ICommand<Result>;
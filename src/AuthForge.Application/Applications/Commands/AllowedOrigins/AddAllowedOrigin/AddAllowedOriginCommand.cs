using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.AllowedOrigins.AddAllowedOrigin;

public sealed record AddAllowedOriginCommand(
    string ApplicationId,
    string Origin) : ICommand<Result>;
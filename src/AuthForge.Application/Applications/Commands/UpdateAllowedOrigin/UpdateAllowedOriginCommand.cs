using AuthForge.Domain.Common;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.UpdateAllowedOrigin;

public record UpdateAllowedOriginCommand(
    ApplicationId ApplicationId,
    string OldOrigin,
    string NewOrigin) : ICommand<Result<UpdateAllowedOriginResponse>>;
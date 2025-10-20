using AuthForge.Domain.Common;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.AllowedOrigins.UpdateAllowedOrigin;

public record UpdateAllowedOriginCommand(
    ApplicationId ApplicationId,
    string OldOrigin,
    string NewOrigin) : ICommand<Result<UpdateAllowedOriginResponse>>;
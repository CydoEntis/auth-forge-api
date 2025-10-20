using AuthForge.Domain.Common;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.AllowedOrigins.RemoveAllowedOrigin;

public record RemoveAllowedOriginCommand(
    ApplicationId ApplicationId,
    string Origin) : ICommand<Result<RemoveAllowedOriginResponse>>;
using AuthForge.Application.Applications.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.UpdateApplication;

public sealed record UpdateApplicationCommand(
    string ApplicationId,
    string? Name,
    string? Description,
    bool? IsActive,
    List<string>? AllowedOrigins,
    EmailSettingsRequest? EmailSettings,
    OAuthSettingsRequest? OAuthSettings) : ICommand<Result<UpdateApplicationResponse>>;
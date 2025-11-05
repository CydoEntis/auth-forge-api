using AuthForge.Application.Applications.Models;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.CreateApplication;

public sealed record CreateApplicationCommand(
    string Name,
    string? Description,
    List<string>? AllowedOrigins,
    EmailSettingsRequest? EmailSettings,
    OAuthSettingsRequest? OAuthSettings) : ICommand<Result<CreateApplicationResponse>>;
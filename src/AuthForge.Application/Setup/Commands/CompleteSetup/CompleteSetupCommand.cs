using AuthForge.Domain.Common;
using AuthForge.Domain.Enums;
using Mediator;

namespace AuthForge.Application.Setup.Commands.CompleteSetup;

public record CompleteSetupCommand(
    DatabaseType DatabaseType,
    string? ConnectionString,
    EmailProvider EmailProvider,
    string? ResendApiKey,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    bool? SmtpUseSsl,
    string FromEmail,
    string FromName,
    string AdminEmail,
    string AdminPassword) : ICommand<Result<CompleteSetupResponse>>;
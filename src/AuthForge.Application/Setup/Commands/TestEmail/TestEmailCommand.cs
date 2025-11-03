using AuthForge.Domain.Common;
using AuthForge.Domain.Enums;
using Mediator;

namespace AuthForge.Application.Setup.Commands.TestEmail;

public record TestEmailCommand(
    EmailProvider Provider,
    string? ResendApiKey,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    bool? SmtpUseSsl,
    string FromEmail,
    string FromName,
    string TestRecipient) : ICommand<Result<TestEmailResponse>>;


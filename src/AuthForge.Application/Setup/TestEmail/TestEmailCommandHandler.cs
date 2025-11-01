using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Admin.Commands.TestEmail;

public sealed class TestEmailCommandHandler
    : ICommandHandler<TestEmailCommand, Result<TestEmailResponse>>
{
    private readonly ISetupService _setupService;
    private readonly ILogger<TestEmailCommandHandler> _logger;

    public TestEmailCommandHandler(
        ISetupService setupService,
        ILogger<TestEmailCommandHandler> logger)
    {
        _setupService = setupService;
        _logger = logger;
    }

    public async ValueTask<Result<TestEmailResponse>> Handle(
        TestEmailCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Testing email configuration: {Provider}", command.Provider);

        var config = new EmailConfiguration(
            command.Provider,
            command.ResendApiKey,
            command.SmtpHost,
            command.SmtpPort,
            command.SmtpUsername,
            command.SmtpPassword,
            command.SmtpUseSsl,
            command.FromEmail,
            command.FromName);

        var isSuccessful = await _setupService
            .TestEmailConnectionAsync(config, command.TestRecipient, cancellationToken);

        var message = isSuccessful
            ? $"Test email sent successfully to {command.TestRecipient}"
            : "Failed to send test email. Please check your configuration.";

        var response = new TestEmailResponse(isSuccessful, message);

        return Result<TestEmailResponse>.Success(response);
    }
}
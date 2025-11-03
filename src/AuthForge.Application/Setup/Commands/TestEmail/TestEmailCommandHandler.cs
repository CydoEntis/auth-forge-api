using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Setup.Commands.TestEmail;

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
            .TestEmailConfigurationAsync(config, command.TestRecipient, cancellationToken);

        if (!isSuccessful)
        {
            _logger.LogWarning("Email test failed for {Provider}", command.Provider);
            
            return Result<TestEmailResponse>.Failure(
                SetupErrors.EmailTestFailed);
        }

        _logger.LogInformation("Email test succeeded for {Provider}", command.Provider);
        
        var response = new TestEmailResponse(
            true, 
            "Test email sent successfully");

        return Result<TestEmailResponse>.Success(response);
    }
}
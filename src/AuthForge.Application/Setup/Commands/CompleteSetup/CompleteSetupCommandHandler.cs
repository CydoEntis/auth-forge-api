using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Models;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Setup.Commands.CompleteSetup;

public sealed class CompleteSetupCommandHandler
    : ICommandHandler<CompleteSetupCommand, Result<CompleteSetupResponse>>
{
    private readonly ISetupService _setupService;
    private readonly ILogger<CompleteSetupCommandHandler> _logger;

    public CompleteSetupCommandHandler(
        ISetupService setupService,
        ILogger<CompleteSetupCommandHandler> logger)
    {
        _setupService = setupService;
        _logger = logger;
    }

    public async ValueTask<Result<CompleteSetupResponse>> Handle(
        CompleteSetupCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting setup completion process");

        if (await _setupService.IsSetupCompleteAsync())
        {
            _logger.LogWarning("Setup already completed");
            return Result<CompleteSetupResponse>.Failure(SetupErrors.SetupAlreadyComplete);
        }

        var databaseConfig = new DatabaseConfiguration(
            command.DatabaseType,
            command.ConnectionString);

        var emailConfig = new EmailConfiguration(
            command.EmailProvider,
            command.ResendApiKey,
            command.SmtpHost,
            command.SmtpPort,
            command.SmtpUsername,
            command.SmtpPassword,
            command.SmtpUseSsl,
            command.FromEmail,
            command.FromName);

        var adminConfig = new AdminSetupConfiguration(
            command.AdminEmail,
            command.AdminPassword);

        var setupConfig = new SetupConfiguration(
            databaseConfig,
            emailConfig,
            adminConfig);

        await _setupService.CompleteSetupAsync(setupConfig, cancellationToken);

        _logger.LogInformation("Setup completed successfully");


        var response = new CompleteSetupResponse("Setup completed successfully.");

        return Result<CompleteSetupResponse>.Success(response);
    }
}
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Applications.Commands.ConfigureApplicationEmailSettings;

public sealed class ConfigureApplicationEmailSettingsCommandHandler
    : ICommandHandler<ConfigureApplicationEmailSettingsCommand, Result<ConfigureApplicationEmailSettingsResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfigureApplicationEmailSettingsCommandHandler> _logger;

    public ConfigureApplicationEmailSettingsCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfigureApplicationEmailSettingsCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<ConfigureApplicationEmailSettingsResponse>> Handle(
        ConfigureApplicationEmailSettingsCommand settingsCommand,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuring email settings for application {ApplicationId} with provider {Provider}",
            settingsCommand.ApplicationId.Value, settingsCommand.Provider);

        var application = await _applicationRepository.GetByIdAsync(
            settingsCommand.ApplicationId,
            cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", settingsCommand.ApplicationId.Value);
            return Result<ConfigureApplicationEmailSettingsResponse>.Failure(ApplicationErrors.NotFound);
        }

        var emailSettings = ApplicationEmailSettings.Create(
            settingsCommand.Provider,
            settingsCommand.ApiKey,
            settingsCommand.FromEmail,
            settingsCommand.FromName,
            settingsCommand.PasswordResetCallbackUrl,
            settingsCommand.EmailVerificationCallbackUrl);

        application.ConfigureEmail(emailSettings);

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully configured email settings for application {ApplicationId} ({ApplicationName}) with provider {Provider}",
            application.Id.Value, application.Name, settingsCommand.Provider);

        return Result<ConfigureApplicationEmailSettingsResponse>.Success(
            new ConfigureApplicationEmailSettingsResponse("Email settings configured successfully."));
    }
}
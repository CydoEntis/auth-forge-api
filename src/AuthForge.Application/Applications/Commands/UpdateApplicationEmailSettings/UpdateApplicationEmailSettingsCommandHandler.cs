using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Applications.Commands.UpdateApplicationEmailSettings;

public sealed class UpdateEmailCommandHandler
    : ICommandHandler<UpdateApplicationEmailSettingsCommand, Result<UpdateApplicationEmailSettingsResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateEmailCommandHandler> _logger;

    public UpdateEmailCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateEmailCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<UpdateApplicationEmailSettingsResponse>> Handle(
        UpdateApplicationEmailSettingsCommand settingsCommand,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating email settings for application {ApplicationId} with provider {Provider}",
            settingsCommand.ApplicationId.Value, settingsCommand.Provider);

        var application = await _applicationRepository.GetByIdAsync(
            settingsCommand.ApplicationId,
            cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", settingsCommand.ApplicationId.Value);
            return Result<UpdateApplicationEmailSettingsResponse>.Failure(ApplicationErrors.NotFound);
        }

        var emailSettings = ApplicationEmailSettings.Create(
            settingsCommand.Provider,
            settingsCommand.ApiKey,
            settingsCommand.FromEmail,
            settingsCommand.FromName);

        application.ConfigureEmail(emailSettings);

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated email settings for application {ApplicationId} ({ApplicationName}) with provider {Provider}",
            application.Id.Value, application.Name, settingsCommand.Provider);

        return Result<UpdateApplicationEmailSettingsResponse>.Success(
            new UpdateApplicationEmailSettingsResponse("Email settings updated successfully."));
    }
}
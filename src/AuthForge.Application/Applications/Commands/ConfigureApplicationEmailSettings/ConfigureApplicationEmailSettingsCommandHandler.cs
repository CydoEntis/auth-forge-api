using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Applications.Commands.ConfigureApplicationEmailSettings;

public sealed class ConfigureApplicationEmailSettingsCommandHandler
    : ICommandHandler<ConfigureApplicationEmailSettingsCommand, Result<ConfigureApplicationEmailSettingsResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureApplicationEmailSettingsCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ConfigureApplicationEmailSettingsResponse>> Handle(
        ConfigureApplicationEmailSettingsCommand settingsCommand,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            settingsCommand.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<ConfigureApplicationEmailSettingsResponse>.Failure(ApplicationErrors.NotFound);

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

        return Result<ConfigureApplicationEmailSettingsResponse>.Success(
            new ConfigureApplicationEmailSettingsResponse("Email settings configured successfully."));
    }
}
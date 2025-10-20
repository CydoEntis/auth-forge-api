using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Applications.Commands.UpdateApplicationEmailSettings;

public sealed class UpdateEmailCommandHandler 
    : ICommandHandler<UpdateApplicationEmailSettingsCommand, Result<UpdateApplicationEmailSettingsResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmailCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<UpdateApplicationEmailSettingsResponse>> Handle(
        UpdateApplicationEmailSettingsCommand settingsCommand,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            settingsCommand.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<UpdateApplicationEmailSettingsResponse>.Failure(ApplicationErrors.NotFound);

        var emailSettings = ApplicationEmailSettings.Create(
            settingsCommand.Provider,
            settingsCommand.ApiKey,
            settingsCommand.FromEmail,
            settingsCommand.FromName);

        application.ConfigureEmail(emailSettings);

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateApplicationEmailSettingsResponse>.Success(
            new UpdateApplicationEmailSettingsResponse("Email settings updated successfully."));
    }
}
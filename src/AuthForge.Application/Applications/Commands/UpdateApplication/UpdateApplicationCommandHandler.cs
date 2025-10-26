using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.UpdateApplication;

public sealed class UpdateApplicationCommandHandler
    : ICommandHandler<UpdateApplicationCommand, Result<UpdateApplicationResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateApplicationCommandHandler> _logger;

    public UpdateApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateApplicationCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<UpdateApplicationResponse>> Handle(
        UpdateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating application {ApplicationId} with name {ApplicationName}",
            command.ApplicationId, command.Name);

        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
        {
            _logger.LogWarning("Invalid application ID format: {ApplicationId}", command.ApplicationId);
            return Result<UpdateApplicationResponse>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));
        }

        var applicationId = ApplicationId.Create(appGuid);

        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", applicationId.Value);
            return Result<UpdateApplicationResponse>.Failure(ApplicationErrors.NotFound);
        }

        if (!application.IsActive)
        {
            _logger.LogWarning("Cannot update inactive application: {ApplicationId}", applicationId.Value);
            return Result<UpdateApplicationResponse>.Failure(ApplicationErrors.Inactive);
        }

        try
        {
            application.UpdateName(command.Name);

            var domainSettings = ApplicationSettings.Create(
                command.Settings.MaxFailedLoginAttempts,
                command.Settings.LockoutDurationMinutes,
                command.Settings.AccessTokenExpirationMinutes,
                command.Settings.RefreshTokenExpirationDays);

            application.UpdateSettings(domainSettings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid settings for application {ApplicationId}: {ErrorMessage}",
                applicationId.Value, ex.Message);
            return Result<UpdateApplicationResponse>.Failure(
                ApplicationErrors.InvalidSettingsDetail(ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated application {ApplicationId} ({ApplicationName})",
            application.Id.Value, application.Name);

        var response = new UpdateApplicationResponse(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.IsActive,
            application.UpdatedAtUtc!.Value);

        return Result<UpdateApplicationResponse>.Success(response);
    }
}
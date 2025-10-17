using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.Update;

public sealed class UpdateApplicationCommandHandler
    : ICommandHandler<UpdateApplicationCommand, Result<UpdateApplicationResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<UpdateApplicationResponse>> Handle(
        UpdateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
            return Result<UpdateApplicationResponse>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));

        if (!Guid.TryParse(command.UserId, out var userGuid))
            return Result<UpdateApplicationResponse>.Failure(ValidationErrors.InvalidGuid("UserId"));

        var applicationId = ApplicationId.Create(appGuid);
        var userId = AuthForgeUserId.Create(userGuid);

        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
            return Result<UpdateApplicationResponse>.Failure(ApplicationErrors.NotFound);

        if (application.UserId != userId)
            return Result<UpdateApplicationResponse>.Failure(ApplicationErrors.Unauthorized);

        if (!application.IsActive)
            return Result<UpdateApplicationResponse>.Failure(ApplicationErrors.Inactive);

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
            return Result<UpdateApplicationResponse>.Failure(
                ApplicationErrors.InvalidSettingsDetail(ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UpdateApplicationResponse(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.IsActive,
            application.UpdatedAtUtc!.Value);

        return Result<UpdateApplicationResponse>.Success(response);
    }
}
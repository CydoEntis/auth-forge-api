using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.Delete;

public sealed class DeleteApplicationCommandHandler
    : ICommandHandler<DeleteApplicationCommand, Result>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(
        DeleteApplicationCommand command,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
            return Result.Failure(ValidationErrors.InvalidGuid("ApplicationId"));

        if (!Guid.TryParse(command.UserId, out var userGuid))
            return Result.Failure(ValidationErrors.InvalidGuid("UserId"));

        var applicationId = ApplicationId.Create(appGuid);
        var userId = AuthForgeUserId.Create(userGuid);

        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
            return Result.Failure(ApplicationErrors.NotFound);

        if (application.UserId != userId)
            return Result.Failure(ApplicationErrors.Unauthorized);

        if (!application.IsActive)
            return Result.Success();

        try
        {
            application.Deactivate();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error(
                "Application.DeactivationFailed",
                ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
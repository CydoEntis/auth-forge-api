using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.DeleteApplication;

public sealed class DeleteApplicationCommandHandler
    : ICommandHandler<DeleteApplicationCommand, Result>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteApplicationCommandHandler> _logger;

    public DeleteApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteApplicationCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(
        DeleteApplicationCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting application {ApplicationId}", command.ApplicationId);

        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
        {
            _logger.LogWarning("Invalid application ID format: {ApplicationId}", command.ApplicationId);
            return Result.Failure(ValidationErrors.InvalidGuid("ApplicationId"));
        }

        var applicationId = ApplicationId.Create(appGuid);

        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", applicationId.Value);
            return Result.Failure(ApplicationErrors.NotFound);
        }

        if (!application.IsActive)
        {
            _logger.LogInformation("Application {ApplicationId} ({ApplicationName}) is already inactive",
                application.Id.Value, application.Name);
            return Result.Success();
        }

        try
        {
            application.Deactivate();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to deactivate application {ApplicationId}: {ErrorMessage}",
                applicationId.Value, ex.Message);
            return Result.Failure(new Error(
                "Application.DeactivationFailed",
                ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deactivated application {ApplicationId} ({ApplicationName})",
            application.Id.Value, application.Name);

        return Result.Success();
    }
}
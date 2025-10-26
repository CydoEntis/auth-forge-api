using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Applications.Commands.UpdateAllowedOrigin;

public sealed class UpdateAllowedOriginCommandHandler
    : ICommandHandler<UpdateAllowedOriginCommand, Result<UpdateAllowedOriginResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAllowedOriginCommandHandler> _logger;

    public UpdateAllowedOriginCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAllowedOriginCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<UpdateAllowedOriginResponse>> Handle(
        UpdateAllowedOriginCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating allowed origin from {OldOrigin} to {NewOrigin} for application {ApplicationId}",
            command.OldOrigin, command.NewOrigin, command.ApplicationId.Value);

        var application = await _applicationRepository.GetByIdAsync(
            command.ApplicationId,
            cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", command.ApplicationId.Value);
            return Result<UpdateAllowedOriginResponse>.Failure(ApplicationErrors.NotFound);
        }

        try
        {
            application.UpdateAllowedOrigin(command.OldOrigin, command.NewOrigin);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid origin for application {ApplicationId}: {ErrorMessage}",
                command.ApplicationId.Value, ex.Message);
            return Result<UpdateAllowedOriginResponse>.Failure(
                ApplicationErrors.InvalidOriginDetail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update origin for application {ApplicationId}: {ErrorMessage}",
                command.ApplicationId.Value, ex.Message);
            return Result<UpdateAllowedOriginResponse>.Failure(
                ApplicationErrors.OriginErrorDetail(ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated allowed origin from {OldOrigin} to {NewOrigin} for application {ApplicationId} ({ApplicationName})",
            command.OldOrigin, command.NewOrigin, application.Id.Value, application.Name);

        return Result<UpdateAllowedOriginResponse>.Success(
            new UpdateAllowedOriginResponse("Allowed origin updated successfully."));
    }
}
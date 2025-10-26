using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Applications.Commands.RemoveAllowedOrigin;

public sealed class RemoveAllowedOriginCommandHandler
    : ICommandHandler<RemoveAllowedOriginCommand, Result<RemoveAllowedOriginResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveAllowedOriginCommandHandler> _logger;

    public RemoveAllowedOriginCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveAllowedOriginCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<RemoveAllowedOriginResponse>> Handle(
        RemoveAllowedOriginCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing allowed origin {Origin} from application {ApplicationId}",
            command.Origin, command.ApplicationId.Value);

        var application = await _applicationRepository.GetByIdAsync(
            command.ApplicationId,
            cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", command.ApplicationId.Value);
            return Result<RemoveAllowedOriginResponse>.Failure(ApplicationErrors.NotFound);
        }

        try
        {
            application.RemoveAllowedOrigin(command.Origin);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid origin for application {ApplicationId}: {ErrorMessage}",
                command.ApplicationId.Value, ex.Message);
            return Result<RemoveAllowedOriginResponse>.Failure(
                ApplicationErrors.InvalidOriginDetail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to remove origin from application {ApplicationId}: {ErrorMessage}",
                command.ApplicationId.Value, ex.Message);
            return Result<RemoveAllowedOriginResponse>.Failure(
                ApplicationErrors.OriginErrorDetail(ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully removed allowed origin {Origin} from application {ApplicationId} ({ApplicationName})",
            command.Origin, application.Id.Value, application.Name);

        return Result<RemoveAllowedOriginResponse>.Success(
            new RemoveAllowedOriginResponse("Allowed origin removed successfully."));
    }
}
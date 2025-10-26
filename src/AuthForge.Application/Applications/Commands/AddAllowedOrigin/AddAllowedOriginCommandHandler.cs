using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.AddAllowedOrigin;

public sealed class AddAllowedOriginCommandHandler : ICommandHandler<AddAllowedOriginCommand, Result>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddAllowedOriginCommandHandler> _logger;

    public AddAllowedOriginCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddAllowedOriginCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result> Handle(AddAllowedOriginCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding allowed origin {Origin} to application {ApplicationId}", command.Origin, command.ApplicationId);

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

        try
        {
            application.AddAllowedOrigin(command.Origin);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid origin for application {ApplicationId}: {ErrorMessage}", applicationId.Value, ex.Message);
            return Result.Failure(ApplicationErrors.InvalidOriginDetail(ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully added allowed origin {Origin} to application {ApplicationId} ({ApplicationName})",
            command.Origin, application.Id.Value, application.Name);

        return Result.Success();
    }
}
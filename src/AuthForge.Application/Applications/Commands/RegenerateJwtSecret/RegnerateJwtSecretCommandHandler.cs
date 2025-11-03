using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.RegenerateJwtSecret;

public sealed class RegenerateJwtSecretCommandHandler
    : ICommandHandler<RegenerateJwtSecretCommand, Result<RegenerateJwtSecretResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegenerateJwtSecretCommandHandler> _logger;

    public RegenerateJwtSecretCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegenerateJwtSecretCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<RegenerateJwtSecretResponse>> Handle(
        RegenerateJwtSecretCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Regenerating JWT secret for application {ApplicationId}", command.ApplicationId);

        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
        {
            _logger.LogWarning("Invalid application ID format: {ApplicationId}", command.ApplicationId);
            return Result<RegenerateJwtSecretResponse>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));
        }

        var applicationId = ApplicationId.Create(appGuid);
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", applicationId.Value);
            return Result<RegenerateJwtSecretResponse>.Failure(ApplicationErrors.NotFound);
        }

        application.RegenerateJwtSecret();

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully regenerated JWT secret for application {ApplicationId} ({ApplicationName})",
            application.Id.Value, application.Name);

        var response = new RegenerateJwtSecretResponse(
            application.JwtSecret,
            DateTime.UtcNow,
            "Save the JWT secret now. You won't be able to see it again.");

        return Result<RegenerateJwtSecretResponse>.Success(response);
    }
}
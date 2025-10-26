using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Applications.Commands.RegenerateApplicationKeys;

public sealed class RegenerateApplicationKeysCommandHandler
    : ICommandHandler<RegenerateApplicationKeysCommand, Result<RegenerateApplicationKeysResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegenerateApplicationKeysCommandHandler> _logger;

    public RegenerateApplicationKeysCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegenerateApplicationKeysCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<RegenerateApplicationKeysResponse>> Handle(
        RegenerateApplicationKeysCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Regenerating keys for application {ApplicationId}", command.ApplicationId.Value);

        var application = await _applicationRepository.GetByIdAsync(command.ApplicationId, cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", command.ApplicationId.Value);
            return Result<RegenerateApplicationKeysResponse>.Failure(ApplicationErrors.NotFound);
        }

        application.RegenerateKeys();

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully regenerated keys for application {ApplicationId} ({ApplicationName})",
            application.Id.Value, application.Name);

        var response = new RegenerateApplicationKeysResponse(
            application.PublicKey,
            application.SecretKey,
            DateTime.UtcNow,
            "Save the secret key now. You won't be able to see it again.");

        return Result<RegenerateApplicationKeysResponse>.Success(response);
    }
}
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
        _logger.LogInformation("Updating application {ApplicationId}", command.ApplicationId);

        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
        {
            _logger.LogWarning("Invalid application ID format: {ApplicationId}", command.ApplicationId);
            return Result<UpdateApplicationResponse>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));
        }

        var applicationId = ApplicationId.Create(appGuid);
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);

        if (application == null)
        {
            _logger.LogWarning("Application not found: {ApplicationId}", applicationId.Value);
            return Result<UpdateApplicationResponse>.Failure(ApplicationErrors.NotFound);
        }

        application.Update(
            command.Name,
            command.Description,
            command.IsActive,
            command.AllowedOrigins);

        if (command.EmailSettings != null)
        {
            var emailSettings = ApplicationEmailSettings.Create(
                command.EmailSettings.Provider,
                command.EmailSettings.ApiKey,
                command.EmailSettings.FromEmail,
                command.EmailSettings.FromName,
                command.EmailSettings.PasswordResetCallbackUrl,
                command.EmailSettings.EmailVerificationCallbackUrl);

            application.ConfigureEmail(emailSettings);
        }

        if (command.OAuthSettings != null)
        {
            var oauthSettings = OAuthSettings.Create(
                command.OAuthSettings.GoogleEnabled,
                command.OAuthSettings.GoogleClientId,
                command.OAuthSettings.GoogleClientSecret,
                command.OAuthSettings.GithubEnabled,
                command.OAuthSettings.GithubClientId,
                command.OAuthSettings.GithubClientSecret);

            application.ConfigureOAuth(oauthSettings);
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated application {ApplicationId} ({ApplicationName})",
            application.Id.Value, application.Name);

        var response = new UpdateApplicationResponse(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.Description,
            application.IsActive,
            application.UpdatedAtUtc ?? DateTime.UtcNow); 

        return Result<UpdateApplicationResponse>.Success(response);
    }
}
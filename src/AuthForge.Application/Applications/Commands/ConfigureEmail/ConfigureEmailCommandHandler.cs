using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Applications.Commands.ConfigureEmail;

public sealed class ConfigureEmailCommandHandler
    : ICommandHandler<ConfigureEmailCommand, Result<ConfigureEmailResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfigureEmailCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ConfigureEmailResponse>> Handle(
        ConfigureEmailCommand command,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            command.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<ConfigureEmailResponse>.Failure(ApplicationErrors.NotFound);

        var emailSettings = ApplicationEmailSettings.Create(
            command.Provider,
            command.ApiKey,
            command.FromEmail,
            command.FromName,
            command.PasswordResetCallbackUrl,
            command.EmailVerificationCallbackUrl);

        application.ConfigureEmail(emailSettings);

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ConfigureEmailResponse>.Success(
            new ConfigureEmailResponse("Email settings configured successfully."));
    }
}
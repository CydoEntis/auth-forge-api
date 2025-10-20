using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Applications.Commands.UpdateEmail;

public sealed class UpdateEmailCommandHandler 
    : ICommandHandler<UpdateEmailCommand, Result<UpdateEmailResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmailCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<UpdateEmailResponse>> Handle(
        UpdateEmailCommand command,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            command.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<UpdateEmailResponse>.Failure(ApplicationErrors.NotFound);

        var emailSettings = ApplicationEmailSettings.Create(
            command.Provider,
            command.ApiKey,
            command.FromEmail,
            command.FromName);

        application.ConfigureEmail(emailSettings);

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateEmailResponse>.Success(
            new UpdateEmailResponse("Email settings updated successfully."));
    }
}
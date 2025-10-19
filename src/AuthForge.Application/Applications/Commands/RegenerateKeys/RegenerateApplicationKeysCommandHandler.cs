using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.Applications.Commands.RegenerateKeys;

public sealed class RegenerateApplicationKeysCommandHandler 
    : ICommandHandler<RegenerateApplicationKeysCommand, Result<RegenerateApplicationKeysResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegenerateApplicationKeysCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RegenerateApplicationKeysResponse>> Handle(
        RegenerateApplicationKeysCommand command, 
        CancellationToken cancellationToken)
    {
        
        var application = await _applicationRepository.GetByIdAsync(command.ApplicationId, cancellationToken);

        if (application == null)
            return Result<RegenerateApplicationKeysResponse>.Failure(ApplicationErrors.NotFound);

        application.RegenerateKeys();

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RegenerateApplicationKeysResponse(
            application.PublicKey,
            application.SecretKey,
            DateTime.UtcNow,
            "Save the secret key now. You won't be able to see it again.");

        return Result<RegenerateApplicationKeysResponse>.Success(response);
    }
}
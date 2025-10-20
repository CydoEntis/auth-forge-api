using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.Applications.Commands.RemoveAllowedOrigin;

public sealed class RemoveAllowedOriginCommandHandler
    : ICommandHandler<RemoveAllowedOriginCommand, Result<RemoveAllowedOriginResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveAllowedOriginCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RemoveAllowedOriginResponse>> Handle(
        RemoveAllowedOriginCommand command,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            command.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<RemoveAllowedOriginResponse>.Failure(ApplicationErrors.NotFound);

        try
        {
            application.RemoveAllowedOrigin(command.Origin);
        }
        catch (ArgumentException ex)
        {
            return Result<RemoveAllowedOriginResponse>.Failure(
                ApplicationErrors.InvalidOriginDetail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Result<RemoveAllowedOriginResponse>.Failure(
                ApplicationErrors.OriginErrorDetail(ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RemoveAllowedOriginResponse>.Success(
            new RemoveAllowedOriginResponse("Allowed origin removed successfully."));
    }
}
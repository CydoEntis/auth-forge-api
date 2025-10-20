using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.Applications.Commands.UpdateAllowedOrigin;

public sealed class UpdateAllowedOriginCommandHandler
    : ICommandHandler<UpdateAllowedOriginCommand, Result<UpdateAllowedOriginResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAllowedOriginCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<UpdateAllowedOriginResponse>> Handle(
        UpdateAllowedOriginCommand command,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            command.ApplicationId,
            cancellationToken);

        if (application == null)
            return Result<UpdateAllowedOriginResponse>.Failure(ApplicationErrors.NotFound);

        try
        {
            application.UpdateAllowedOrigin(command.OldOrigin, command.NewOrigin);
        }
        catch (ArgumentException ex)
        {
            return Result<UpdateAllowedOriginResponse>.Failure(
                new Error("Application.InvalidOrigin", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Result<UpdateAllowedOriginResponse>.Failure(
                new Error("Application.OriginError", ex.Message));
        }

        _applicationRepository.Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateAllowedOriginResponse>.Success(
            new UpdateAllowedOriginResponse("Allowed origin updated successfully."));
    }
}
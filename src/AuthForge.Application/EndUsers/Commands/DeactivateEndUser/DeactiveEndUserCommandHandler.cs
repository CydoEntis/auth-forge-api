using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.DeactivateEndUser;

public sealed class DeactivateEndUserCommandHandler
    : ICommandHandler<DeactivateEndUserCommand, Result<DeactivateEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<DeactivateEndUserResponse>> Handle(
        DeactivateEndUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<DeactivateEndUserResponse>.Failure(EndUserErrors.NotFound);

        try
        {
            user.Deactivate();
        }
        catch (InvalidOperationException)
        {
            return Result<DeactivateEndUserResponse>.Failure(EndUserErrors.AlreadyDeactivated); 
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeactivateEndUserResponse>.Success(
            new DeactivateEndUserResponse("User deactivated successfully."));
    }
}
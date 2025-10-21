using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ActivateEndUser;

public sealed class ActivateEndUserCommandHandler
    : ICommandHandler<ActivateEndUserCommand, Result<ActivateEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ActivateEndUserResponse>> Handle(
        ActivateEndUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<ActivateEndUserResponse>.Failure(EndUserErrors.NotFound);

        try
        {
            user.Activate();
        }
        catch (InvalidOperationException)
        {
            return Result<ActivateEndUserResponse>.Failure(EndUserErrors.AlreadyActive); 
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ActivateEndUserResponse>.Success(
            new ActivateEndUserResponse("User activated successfully."));
    }
}
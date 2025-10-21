using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.UnlockEndUser;

public sealed class UnlockEndUserCommandHandler 
    : ICommandHandler<UnlockEndUserCommand, Result<UnlockEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnlockEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<UnlockEndUserResponse>> Handle(
        UnlockEndUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<UnlockEndUserResponse>.Failure(EndUserErrors.NotFound);

        if (!user.IsLockedOut())
            return Result<UnlockEndUserResponse>.Failure(EndUserErrors.NotLockedOut);

        user.Unlock();

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UnlockEndUserResponse>.Success(
            new UnlockEndUserResponse("User account unlocked successfully."));
    }
}
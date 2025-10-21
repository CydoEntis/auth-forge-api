using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.LockEndUser;

public sealed class LockEndUserCommandHandler 
    : ICommandHandler<LockEndUserCommand, Result<LockEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LockEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<LockEndUserResponse>> Handle(
        LockEndUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<LockEndUserResponse>.Failure(EndUserErrors.NotFound);

        try
        {
            user.ManualLock(command.LockoutMinutes);
        }
        catch (InvalidOperationException)
        {
            return Result<LockEndUserResponse>.Failure(EndUserErrors.AlreadyLockedOut);
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LockEndUserResponse>.Success(
            new LockEndUserResponse(
                "User account locked successfully.",
                user.LockedOutUntil!.Value));
    }
}
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.DeleteEndUser;

public sealed class DeleteEndUserCommandHandler 
    : ICommandHandler<DeleteEndUserCommand, Result<DeleteEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<DeleteEndUserResponse>> Handle(
        DeleteEndUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<DeleteEndUserResponse>.Failure(EndUserErrors.NotFound);

        _endUserRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeleteEndUserResponse>.Success(
            new DeleteEndUserResponse("User deleted successfully."));
    }
}
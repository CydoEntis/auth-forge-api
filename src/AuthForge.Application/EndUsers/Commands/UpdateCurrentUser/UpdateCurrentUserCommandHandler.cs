using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.UpdateCurrentUser;

public sealed class UpdateCurrentUserCommandHandler 
    : ICommandHandler<UpdateCurrentUserCommand, Result<UpdateCurrentUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCurrentUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<UpdateCurrentUserResponse>> Handle(
        UpdateCurrentUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<UpdateCurrentUserResponse>.Failure(EndUserErrors.NotFound);

        try
        {
            user.UpdateProfile(command.FirstName, command.LastName);
        }
        catch (ArgumentException ex)
        {
            return Result<UpdateCurrentUserResponse>.Failure(
                new Error("EndUser.InvalidInput", ex.Message));
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateCurrentUserResponse>.Success(
            new UpdateCurrentUserResponse("Profile updated successfully."));
    }
}
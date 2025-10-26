using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.DeleteEndUser;

public sealed class DeleteEndUserCommandHandler
    : ICommandHandler<DeleteEndUserCommand, Result<DeleteEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteEndUserCommandHandler> _logger;

    public DeleteEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteEndUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }


    public async ValueTask<Result<DeleteEndUserResponse>> Handle(
        DeleteEndUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for deletion", command.UserId);
            return Result<DeleteEndUserResponse>.Failure(EndUserErrors.NotFound);
        }

        var userId = user.Id;
        var userEmail = user.Email;

        user.Delete();

        _endUserRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) successfully deleted", userId, userEmail);

        return Result<DeleteEndUserResponse>.Success(
            new DeleteEndUserResponse("User deleted successfully."));
    }
}
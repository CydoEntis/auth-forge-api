using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.DeactivateEndUser;

public sealed class DeactivateEndUserCommandHandler
    : ICommandHandler<DeactivateEndUserCommand, Result<DeactivateEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateEndUserCommandHandler> _logger;

    public DeactivateEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateEndUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<DeactivateEndUserResponse>> Handle(
        DeactivateEndUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for deactivation", command.UserId);
            return Result<DeactivateEndUserResponse>.Failure(EndUserErrors.NotFound);
        }

        try
        {
            user.Deactivate();
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("User {UserId} ({Email}) is already deactivated", user.Id, user.Email);
            return Result<DeactivateEndUserResponse>.Failure(EndUserErrors.AlreadyDeactivated);
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) successfully deactivated", user.Id, user.Email);

        return Result<DeactivateEndUserResponse>.Success(
            new DeactivateEndUserResponse("User deactivated successfully."));
    }
}
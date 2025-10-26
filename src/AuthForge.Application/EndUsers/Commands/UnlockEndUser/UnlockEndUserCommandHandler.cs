using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.UnlockEndUser;

public sealed class UnlockEndUserCommandHandler
    : ICommandHandler<UnlockEndUserCommand, Result<UnlockEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnlockEndUserCommandHandler> _logger;

    public UnlockEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<UnlockEndUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<UnlockEndUserResponse>> Handle(
        UnlockEndUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to unlock user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for unlocking", command.UserId);
            return Result<UnlockEndUserResponse>.Failure(EndUserErrors.NotFound);
        }

        if (!user.IsLockedOut())
        {
            _logger.LogWarning("User {UserId} ({Email}) is not locked out", user.Id, user.Email);
            return Result<UnlockEndUserResponse>.Failure(EndUserErrors.NotLockedOut);
        }

        user.Unlock();

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) successfully unlocked", user.Id, user.Email);

        return Result<UnlockEndUserResponse>.Success(
            new UnlockEndUserResponse("User account unlocked successfully."));
    }
}
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.LockEndUser;

public sealed class LockEndUserCommandHandler
    : ICommandHandler<LockEndUserCommand, Result<LockEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LockEndUserCommandHandler> _logger;

    public LockEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<LockEndUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<LockEndUserResponse>> Handle(
        LockEndUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to lock user {UserId} for {LockoutMinutes} minutes",
            command.UserId, command.LockoutMinutes);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for locking", command.UserId);
            return Result<LockEndUserResponse>.Failure(EndUserErrors.NotFound);
        }

        try
        {
            user.ManualLock(command.LockoutMinutes);
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("User {UserId} ({Email}) is already locked out", user.Id, user.Email);
            return Result<LockEndUserResponse>.Failure(EndUserErrors.AlreadyLockedOut);
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) locked by admin until {LockedOutUntil}",
            user.Id, user.Email, user.LockedOutUntil);

        return Result<LockEndUserResponse>.Success(
            new LockEndUserResponse(
                "User account locked successfully.",
                user.LockedOutUntil!.Value));
    }
}
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.ActivateEndUser;

public sealed class ActivateEndUserCommandHandler
    : ICommandHandler<ActivateEndUserCommand, Result<ActivateEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateEndUserCommandHandler> _logger;

    public ActivateEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivateEndUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<ActivateEndUserResponse>> Handle(
        ActivateEndUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for activation", command.UserId);
            return Result<ActivateEndUserResponse>.Failure(EndUserErrors.NotFound);
        }

        try
        {
            user.Activate();
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("User {UserId} ({Email}) is already active", user.Id, user.Email);
            return Result<ActivateEndUserResponse>.Failure(EndUserErrors.AlreadyActive);
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) successfully activated", user.Id, user.Email);

        return Result<ActivateEndUserResponse>.Success(
            new ActivateEndUserResponse("User activated successfully."));
    }
}
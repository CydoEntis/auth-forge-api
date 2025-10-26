using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.UpdateCurrentUser;

public sealed class UpdateCurrentUserCommandHandler
    : ICommandHandler<UpdateCurrentUserCommand, Result<UpdateCurrentUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCurrentUserCommandHandler> _logger;

    public UpdateCurrentUserCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCurrentUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<UpdateCurrentUserResponse>> Handle(
        UpdateCurrentUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating profile for user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for profile update", command.UserId);
            return Result<UpdateCurrentUserResponse>.Failure(EndUserErrors.NotFound);
        }

        try
        {
            user.UpdateProfile(command.FirstName, command.LastName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid input for user {UserId} ({Email}) profile update: {Message}",
                user.Id, user.Email, ex.Message);
            return Result<UpdateCurrentUserResponse>.Failure(
                new Error("EndUser.InvalidInput", ex.Message));
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profile successfully updated for user {UserId} ({Email})", user.Id, user.Email);

        return Result<UpdateCurrentUserResponse>.Success(
            new UpdateCurrentUserResponse("Profile updated successfully."));
    }
}
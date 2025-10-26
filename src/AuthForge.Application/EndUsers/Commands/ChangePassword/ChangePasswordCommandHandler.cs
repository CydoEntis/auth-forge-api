using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler
    : ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IEndUserRepository endUserRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<ChangePasswordResponse>> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password change attempt for user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for password change", command.UserId);
            return Result<ChangePasswordResponse>.Failure(EndUserErrors.NotFound);
        }

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Invalid current password provided for user {UserId} ({Email})", user.Id, user.Email);
            return Result<ChangePasswordResponse>.Failure(EndUserErrors.InvalidCredentials);
        }

        var hashedPassword = _passwordHasher.HashPassword(command.NewPassword);
        user.UpdatePassword(hashedPassword);

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password successfully changed for user {UserId} ({Email})", user.Id, user.Email);

        return Result<ChangePasswordResponse>.Success(
            new ChangePasswordResponse("Password changed successfully."));
    }
}
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : ICommandHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IEndUserRepository endUserRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<ResetPasswordResponse>> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password reset attempt for email {Email} in application {ApplicationId}",
            command.Email, command.ApplicationId);

        var user = await _endUserRepository.GetByEmailAsync(
            command.ApplicationId,
            command.Email,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Password reset attempted for non-existent email {Email} in application {ApplicationId}",
                command.Email, command.ApplicationId);
            return Result<ResetPasswordResponse>.Failure(EndUserErrors.NotFound);
        }

        if (!user.IsPasswordResetTokenValid(command.ResetToken))
        {
            _logger.LogWarning("Invalid or expired reset token used for user {UserId} ({Email})", user.Id, user.Email);
            return Result<ResetPasswordResponse>.Failure(EndUserErrors.InvalidResetToken);
        }

        var hashedPassword = _passwordHasher.HashPassword(command.NewPassword);
        user.ResetPassword(hashedPassword);

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password successfully reset for user {UserId} ({Email})", user.Id, user.Email);

        return Result<ResetPasswordResponse>.Success(
            new ResetPasswordResponse("Password has been reset successfully."));
    }
}
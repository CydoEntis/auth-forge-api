using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : ICommandHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Password reset requested for email {Email} in application {ApplicationId}",
            command.Email, command.ApplicationId);

        var user = await _endUserRepository.GetByEmailAsync(
            command.ApplicationId,
            command.Email,
            cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("Password reset requested for non-existent email {Email} in application {ApplicationId}",
                command.Email, command.ApplicationId);
            return Result<ForgotPasswordResponse>.Success(
                new ForgotPasswordResponse("If an account exists, a password reset email has been sent."));
        }

        var resetToken = GenerateResetToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        user.SetPasswordResetToken(resetToken, expiresAt);

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset token generated for user {UserId} ({Email})", user.Id, user.Email);

        return Result<ForgotPasswordResponse>.Success(
            new ForgotPasswordResponse("If an account exists, a password reset email has been sent."));
    }

    private static string GenerateResetToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }
}
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler 
    : ICommandHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByEmailAsync(
            command.ApplicationId,
            command.Email,
            cancellationToken);

        if (user == null)
        {
            return Result<ForgotPasswordResponse>.Success(
                new ForgotPasswordResponse("If an account exists, a password reset email has been sent."));
        }

        var resetToken = GenerateResetToken();
        var expiresAt = DateTime.UtcNow.AddHours(1); 

        user.SetPasswordResetToken(resetToken, expiresAt);
        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Send email with reset token

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
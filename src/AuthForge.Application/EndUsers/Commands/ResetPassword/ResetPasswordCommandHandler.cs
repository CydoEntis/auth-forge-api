using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler 
    : ICommandHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IEndUserRepository endUserRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ResetPasswordResponse>> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByEmailAsync(
            command.ApplicationId,
            command.Email,
            cancellationToken);

        if (user == null)
            return Result<ResetPasswordResponse>.Failure(EndUserErrors.NotFound);

        if (!user.IsPasswordResetTokenValid(command.ResetToken))
            return Result<ResetPasswordResponse>.Failure(EndUserErrors.InvalidResetToken);

        var hashedPassword = _passwordHasher.HashPassword(command.NewPassword);
        user.ResetPassword(hashedPassword);

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ResetPasswordResponse>.Success(
            new ResetPasswordResponse("Password has been reset successfully."));
    }
}
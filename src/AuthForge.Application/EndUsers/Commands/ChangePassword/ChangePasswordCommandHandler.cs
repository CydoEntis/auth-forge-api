using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler 
    : ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        IEndUserRepository endUserRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ChangePasswordResponse>> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<ChangePasswordResponse>.Failure(EndUserErrors.NotFound);

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            return Result<ChangePasswordResponse>.Failure(EndUserErrors.InvalidCredentials);

        var hashedPassword = _passwordHasher.HashPassword(command.NewPassword);
        user.UpdatePassword(hashedPassword);

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChangePasswordResponse>.Success(
            new ChangePasswordResponse("Password changed successfully."));
    }
}
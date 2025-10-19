using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler 
    : ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<VerifyEmailResponse>> Handle(
        VerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByEmailAsync(
            command.ApplicationId,
            command.Email,
            cancellationToken);

        if (user == null)
            return Result<VerifyEmailResponse>.Failure(EndUserErrors.NotFound);

        if (user.IsEmailVerified)
            return Result<VerifyEmailResponse>.Failure(EndUserErrors.EmailAlreadyVerified);

        if (!user.IsEmailVerificationTokenValid(command.VerificationToken))
            return Result<VerifyEmailResponse>.Failure(EndUserErrors.InvalidVerificationToken);

        user.VerifyEmail();

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VerifyEmailResponse>.Success(
            new VerifyEmailResponse("Email verified successfully."));
    }
}
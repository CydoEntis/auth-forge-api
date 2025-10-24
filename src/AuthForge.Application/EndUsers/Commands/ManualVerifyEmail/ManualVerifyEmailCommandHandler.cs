using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ManualVerifyEmail;

public sealed class ManualVerifyEmailCommandHandler 
    : ICommandHandler<ManualVerifyEmailCommand, Result<ManualVerifyEmailResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ManualVerifyEmailCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ManualVerifyEmailResponse>> Handle(
        ManualVerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<ManualVerifyEmailResponse>.Failure(EndUserErrors.NotFound);

        try
        {
            user.VerifyEmailManually();
        }
        catch (InvalidOperationException)
        {
            return Result<ManualVerifyEmailResponse>.Failure(EndUserErrors.EmailAlreadyVerified);
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ManualVerifyEmailResponse>.Success(
            new ManualVerifyEmailResponse("User email verified successfully by admin."));
    }
}
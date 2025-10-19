using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.SendVerificationEmail;

public sealed class SendVerificationEmailCommandHandler 
    : ICommandHandler<SendVerificationEmailCommand, Result<SendVerificationEmailResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendVerificationEmailCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<SendVerificationEmailResponse>> Handle(
        SendVerificationEmailCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
            return Result<SendVerificationEmailResponse>.Failure(EndUserErrors.NotFound);

        if (user.IsEmailVerified)
            return Result<SendVerificationEmailResponse>.Failure(EndUserErrors.EmailAlreadyVerified);

        var verificationToken = GenerateVerificationToken();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        user.SetEmailVerificationToken(verificationToken, expiresAt);
        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Send email with verification token

        return Result<SendVerificationEmailResponse>.Success(
            new SendVerificationEmailResponse("Verification email sent successfully."));
    }

    private static string GenerateVerificationToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }
}
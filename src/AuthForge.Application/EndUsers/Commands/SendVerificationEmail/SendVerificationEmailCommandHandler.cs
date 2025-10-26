using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.SendVerificationEmail;

public sealed class SendVerificationEmailCommandHandler
    : ICommandHandler<SendVerificationEmailCommand, Result<SendVerificationEmailResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendVerificationEmailCommandHandler> _logger;

    public SendVerificationEmailCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<SendVerificationEmailCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<SendVerificationEmailResponse>> Handle(
        SendVerificationEmailCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending verification email for user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for email verification", command.UserId);
            return Result<SendVerificationEmailResponse>.Failure(EndUserErrors.NotFound);
        }

        if (user.IsEmailVerified)
        {
            _logger.LogWarning("User {UserId} ({Email}) email is already verified", user.Id, user.Email);
            return Result<SendVerificationEmailResponse>.Failure(EndUserErrors.EmailAlreadyVerified);
        }

        var verificationToken = GenerateVerificationToken();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        user.SetEmailVerificationToken(verificationToken, expiresAt);

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Verification email token generated for user {UserId} ({Email})", user.Id, user.Email);

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
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler
    : ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<VerifyEmailResponse>> Handle(
        VerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email verification attempt for {Email} in application {ApplicationId}",
            command.Email, command.ApplicationId);

        var user = await _endUserRepository.GetByEmailAsync(
            command.ApplicationId,
            command.Email,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Email verification attempted for non-existent email {Email} in application {ApplicationId}",
                command.Email, command.ApplicationId);
            return Result<VerifyEmailResponse>.Failure(EndUserErrors.NotFound);
        }

        if (user.IsEmailVerified)
        {
            _logger.LogWarning("Email verification attempted for already verified user {UserId} ({Email})",
                user.Id, user.Email);
            return Result<VerifyEmailResponse>.Failure(EndUserErrors.EmailAlreadyVerified);
        }

        if (!user.IsEmailVerificationTokenValid(command.VerificationToken))
        {
            _logger.LogWarning("Invalid or expired verification token used for user {UserId} ({Email})",
                user.Id, user.Email);
            return Result<VerifyEmailResponse>.Failure(EndUserErrors.InvalidVerificationToken);
        }

        user.VerifyEmail();

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email verification successful for user {UserId} ({Email})", user.Id, user.Email);

        return Result<VerifyEmailResponse>.Success(
            new VerifyEmailResponse("Email verified successfully."));
    }
}
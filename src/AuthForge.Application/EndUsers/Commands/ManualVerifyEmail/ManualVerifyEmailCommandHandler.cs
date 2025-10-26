using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.EndUsers.Commands.ManualVerifyEmail;

public sealed class ManualVerifyEmailCommandHandler
    : ICommandHandler<ManualVerifyEmailCommand, Result<ManualVerifyEmailResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ManualVerifyEmailCommandHandler> _logger;

    public ManualVerifyEmailCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<ManualVerifyEmailCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<ManualVerifyEmailResponse>> Handle(
        ManualVerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin attempting to manually verify email for user {UserId}", command.UserId);

        var user = await _endUserRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for manual email verification", command.UserId);
            return Result<ManualVerifyEmailResponse>.Failure(EndUserErrors.NotFound);
        }

        try
        {
            user.VerifyEmailManually();
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("User {UserId} ({Email}) email is already verified", user.Id, user.Email);
            return Result<ManualVerifyEmailResponse>.Failure(EndUserErrors.EmailAlreadyVerified);
        }

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) email verified manually by admin", user.Id, user.Email);

        return Result<ManualVerifyEmailResponse>.Success(
            new ManualVerifyEmailResponse("User email verified successfully by admin."));
    }
}
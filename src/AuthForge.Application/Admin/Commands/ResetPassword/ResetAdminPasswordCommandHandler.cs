using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Admin.Commands.ResetPassword;

public sealed class ResetAdminPasswordCommandHandler 
    : ICommandHandler<ResetAdminPasswordCommand, Result<ResetAdminPasswordResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetAdminPasswordCommandHandler> _logger;

    public ResetAdminPasswordCommandHandler(
        IAdminRepository adminRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<ResetAdminPasswordCommandHandler> logger)
    {
        _adminRepository = adminRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<ResetAdminPasswordResponse>> Handle(
        ResetAdminPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var admin = await _adminRepository.GetByPasswordResetTokenAsync(
            command.ResetToken,
            cancellationToken);

        if (admin is null || !admin.IsPasswordResetTokenValid(command.ResetToken))
        {
            _logger.LogWarning(
                "Invalid or expired password reset token attempted");
            
            return Result<ResetAdminPasswordResponse>.Failure(
                AdminErrors.InvalidResetToken);
        }

        var hashedPassword = HashedPassword.Create(command.NewPassword);
        admin.UpdatePassword(hashedPassword);

        _adminRepository.Update(admin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin password successfully reset for {Email}",
            admin.Email.Value);

        return Result<ResetAdminPasswordResponse>.Success(
            new ResetAdminPasswordResponse("Password has been reset successfully."));
    }
}
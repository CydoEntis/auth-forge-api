using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Admin.Commands.ChangePassword;

public sealed class ChangeAdminPasswordCommandHandler 
    : ICommandHandler<ChangeAdminPasswordCommand, Result<ChangeAdminPasswordResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChangeAdminPasswordCommandHandler> _logger;

    public ChangeAdminPasswordCommandHandler(
        IAdminRepository adminRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ChangeAdminPasswordCommandHandler> logger)
    {
        _adminRepository = adminRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async ValueTask<Result<ChangeAdminPasswordResponse>> Handle(
        ChangeAdminPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var email = _currentUserService.Email;
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Attempted to change password without authentication");
            return Result<ChangeAdminPasswordResponse>.Failure(AdminErrors.Unauthorized);
        }

        _logger.LogInformation("Password change attempt for admin {Email}", email);

        var admin = await _adminRepository.GetByEmailAsync(
            Email.Create(email),
            cancellationToken);

        if (admin is null)
        {
            _logger.LogWarning("Admin {Email} not found for password change", email);
            return Result<ChangeAdminPasswordResponse>.Failure(AdminErrors.NotFound);
        }

        if (!_passwordHasher.VerifyPassword(command.CurrentPassword, admin.PasswordHash))
        {
            _logger.LogWarning(
                "Invalid current password provided for admin {AdminId} ({Email})", 
                admin.Id, 
                admin.Email.Value);
            return Result<ChangeAdminPasswordResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        var hashedPassword = _passwordHasher.HashPassword(command.NewPassword);
        admin.UpdatePassword(hashedPassword);

        _adminRepository.Update(admin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Password successfully changed for admin {AdminId} ({Email}) from IP {IpAddress}",
            admin.Id,
            admin.Email.Value,
            _currentUserService.IpAddress);

        return Result<ChangeAdminPasswordResponse>.Success(
            new ChangeAdminPasswordResponse("Password changed successfully"));
    }
}
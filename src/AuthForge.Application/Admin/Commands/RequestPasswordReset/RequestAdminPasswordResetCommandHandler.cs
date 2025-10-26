using System.Security.Cryptography;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

namespace AuthForge.Application.Admin.Commands.RequestPasswordReset;

public sealed class RequestAdminPasswordResetCommandHandler
    : ICommandHandler<RequestAdminPasswordResetCommand, Result<RequestAdminPasswordResetResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly ISystemEmailService _systemEmailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestAdminPasswordResetCommandHandler> _logger;

    public RequestAdminPasswordResetCommandHandler(
        IAdminRepository adminRepository,
        ISystemEmailService systemEmailService,
        IUnitOfWork unitOfWork,
        ILogger<RequestAdminPasswordResetCommandHandler> logger)
    {
        _adminRepository = adminRepository;
        _systemEmailService = systemEmailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<RequestAdminPasswordResetResponse>> Handle(
        RequestAdminPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        const string successMessage =
            "If an admin account exists with that email, a password reset link has been sent.";

        Email email;
        try
        {
            email = Email.Create(command.Email);
        }
        catch (ArgumentException)
        {
            _logger.LogInformation(
                "Password reset requested with invalid email format: {Email}",
                command.Email);

            return Result<RequestAdminPasswordResetResponse>.Success(
                new RequestAdminPasswordResetResponse(successMessage));
        }

        var admin = await _adminRepository.GetByEmailAsync(email, cancellationToken);

        if (admin is null)
        {
            _logger.LogInformation(
                "Password reset requested for non-existent admin email: {Email}",
                command.Email);

            return Result<RequestAdminPasswordResetResponse>.Success(
                new RequestAdminPasswordResetResponse(successMessage));
        }

        var resetToken = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        admin.SetPasswordResetToken(resetToken, expiresAt);
        _adminRepository.Update(admin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_systemEmailService.IsConfigured())
        {
            try
            {
                await _systemEmailService.SendAdminPasswordResetEmailAsync(
                    admin.Email.Value,
                    resetToken,
                    cancellationToken);

                _logger.LogInformation(
                    "Admin password reset email sent to {Email}",
                    admin.Email.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send admin password reset email to {Email}",
                    admin.Email.Value);
            }
        }
        else
        {
            _logger.LogWarning(
                "⚠️  Admin password reset requested but system email not configured.\n" +
                "Email: {Email}\n" +
                "Reset Token: {Token}\n" +
                "Expires At: {ExpiresAt} UTC\n" +
                "Admin must manually reset using this token or configure system email.",
                admin.Email.Value,
                resetToken,
                expiresAt);
        }

        return Result<RequestAdminPasswordResetResponse>.Success(
            new RequestAdminPasswordResetResponse(successMessage));
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
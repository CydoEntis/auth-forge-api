using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Settings;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthForge.Application.Admin.Commands.Login;

public sealed class LoginAdminCommandHandler
    : ICommandHandler<LoginAdminCommand, Result<LoginAdminResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IAdminRefreshTokenRepository _refreshTokenRepository;
    private readonly IAdminJwtTokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuthForgeSettings _settings;
    private readonly ILogger<LoginAdminCommandHandler> _logger;

    public LoginAdminCommandHandler(
        IAdminRepository adminRepository,
        IAdminRefreshTokenRepository refreshTokenRepository,
        IAdminJwtTokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<AuthForgeSettings> settings,
        ILogger<LoginAdminCommandHandler> logger)
    {
        _adminRepository = adminRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _logger = logger;
    }

    public async ValueTask<Result<LoginAdminResponse>> Handle(
        LoginAdminCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin login attempt for email {Email}", command.Email);

        var admin = await _adminRepository.GetByEmailAsync(
            Email.Create(command.Email),
            cancellationToken);

        if (admin == null)
        {
            _logger.LogWarning("Admin login failed - Admin not found for email {Email}", command.Email);
            return Result<LoginAdminResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        if (!admin.PasswordHash.Verify(command.Password))
        {
            _logger.LogWarning("Admin login failed - Invalid password for {AdminId}. Failed attempts: {FailedAttempts}",
                admin.Id, admin.FailedLoginAttempts + 1);

            admin.RecordFailedLogin(5, 15);
            _adminRepository.Update(admin);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<LoginAdminResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        if (admin.IsLockedOut())
        {
            _logger.LogWarning("Admin login failed - Admin {AdminId} is locked out", admin.Id);
            return Result<LoginAdminResponse>.Failure(AdminErrors.LockedOut);
        }

        admin.RecordSuccessfulLogin();
        _adminRepository.Update(admin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenGenerator.GenerateAccessToken(admin.Email.Value);
        var refreshTokenString = _tokenGenerator.GenerateRefreshToken();

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_settings.Jwt.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_settings.Jwt.RefreshTokenExpirationDays);

        var refreshToken = AdminRefreshToken.Create(
            admin.Id,
            refreshTokenString,
            refreshTokenExpiresAt);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin {AdminId} ({Email}) successfully logged in", admin.Id, admin.Email.Value);

        var response = new LoginAdminResponse(
            accessToken,
            refreshTokenString,
            accessTokenExpiresAt);

        return Result<LoginAdminResponse>.Success(response);
    }
}
using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Settings;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
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

    public LoginAdminCommandHandler(
        IAdminRepository adminRepository,
        IAdminRefreshTokenRepository refreshTokenRepository,
        IAdminJwtTokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<AuthForgeSettings> settings)
    {
        _adminRepository = adminRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
    }

    public async ValueTask<Result<LoginAdminResponse>> Handle(
        LoginAdminCommand command,
        CancellationToken cancellationToken)
    {
        var admin = await _adminRepository.GetByEmailAsync(
            Email.Create(command.Email),
            cancellationToken);

        if (admin == null)
            return Result<LoginAdminResponse>.Failure(AdminErrors.InvalidCredentials);

        if (!admin.PasswordHash.Verify(command.Password))
        {
            admin.RecordFailedLogin(5, 15);
            _adminRepository.Update(admin);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<LoginAdminResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        if (admin.IsLockedOut())
            return Result<LoginAdminResponse>.Failure(AdminErrors.LockedOut);

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

        var response = new LoginAdminResponse(
            accessToken,
            refreshTokenString,
            accessTokenExpiresAt);

        return Result<LoginAdminResponse>.Success(response);
    }
}
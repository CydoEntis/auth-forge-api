using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Settings;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthForge.Application.Admin.Commands.Refresh;

public sealed class RefreshAdminTokenCommandHandler
    : ICommandHandler<RefreshAdminTokenCommand, Result<RefreshAdminTokenResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IAdminRefreshTokenRepository _refreshTokenRepository;
    private readonly IAdminJwtTokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuthForgeSettings _settings;
    private readonly ILogger<RefreshAdminTokenCommandHandler> _logger;

    public RefreshAdminTokenCommandHandler(
        IAdminRepository adminRepository,
        IAdminRefreshTokenRepository refreshTokenRepository,
        IAdminJwtTokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<AuthForgeSettings> settings,
        ILogger<RefreshAdminTokenCommandHandler> logger)
    {
        _adminRepository = adminRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _logger = logger;
    }

    public async ValueTask<Result<RefreshAdminTokenResponse>> Handle(
        RefreshAdminTokenCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Admin token refresh attempt");

        var refreshToken = await _refreshTokenRepository
            .GetByTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            _logger.LogWarning("Admin token refresh failed - Invalid or inactive refresh token");
            return Result<RefreshAdminTokenResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        var admin = await _adminRepository.GetByIdAsync(
            refreshToken.AdminId,
            cancellationToken);

        if (admin is null)
        {
            _logger.LogError("Admin token refresh failed - Admin not found for AdminId {AdminId}", refreshToken.AdminId);
            return Result<RefreshAdminTokenResponse>.Failure(AdminErrors.NotFound);
        }

        refreshToken.MarkAsUsed();

        var newAccessToken = _tokenGenerator.GenerateAccessToken(admin.Email.Value);
        var newRefreshTokenString = _tokenGenerator.GenerateRefreshToken();

        var newRefreshTokenExpiresAt = DateTime.UtcNow
            .AddDays(_settings.Jwt.RefreshTokenExpirationDays);

        var newRefreshToken = AdminRefreshToken.Create(
            admin.Id,
            newRefreshTokenString,
            newRefreshTokenExpiresAt);

        refreshToken.Revoke(newRefreshTokenString);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        _refreshTokenRepository.Update(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin token successfully refreshed for {AdminId} ({Email})", admin.Id, admin.Email.Value);

        var response = new RefreshAdminTokenResponse(
            newAccessToken,
            newRefreshTokenString,
            DateTime.UtcNow.AddMinutes(_settings.Jwt.AccessTokenExpirationMinutes));

        return Result<RefreshAdminTokenResponse>.Success(response);
    }
}
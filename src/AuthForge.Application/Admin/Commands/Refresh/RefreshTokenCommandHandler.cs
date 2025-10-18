using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Settings;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using Mediator;
using Microsoft.Extensions.Options;

namespace AuthForge.Application.Admin.Commands.Refresh;

public sealed class RefreshAdminTokenCommandHandler
    : ICommandHandler<RefreshAdminTokenCommand, Result<RefreshAdminTokenResponse>>
{
    private readonly IOptions<AuthForgeSettings> _settings;
    private readonly IAdminJwtTokenGenerator _tokenGenerator;
    private readonly IAdminRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshAdminTokenCommandHandler(
        IOptions<AuthForgeSettings> settings,
        IAdminJwtTokenGenerator tokenGenerator,
        IAdminRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _settings = settings;
        _tokenGenerator = tokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RefreshAdminTokenResponse>> Handle(
        RefreshAdminTokenCommand command,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository
            .GetByTokenAsync(command.RefreshToken, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return Result<RefreshAdminTokenResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        refreshToken.MarkAsUsed();

        var adminEmail = _settings.Value.Admin.Email;
        var newAccessToken = _tokenGenerator.GenerateAccessToken(adminEmail);
        var newRefreshTokenString = _tokenGenerator.GenerateRefreshToken();

        var newRefreshTokenExpiresAt = DateTime.UtcNow
            .AddDays(_settings.Value.Jwt.RefreshTokenExpirationDays);

        var newRefreshToken = AdminRefreshToken.Create(
            newRefreshTokenString,
            newRefreshTokenExpiresAt);

        refreshToken.Revoke(newRefreshTokenString);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        _refreshTokenRepository.Update(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RefreshAdminTokenResponse(
            newAccessToken,
            newRefreshTokenString,
            DateTime.UtcNow.AddMinutes(_settings.Value.Jwt.AccessTokenExpirationMinutes));

        return Result<RefreshAdminTokenResponse>.Success(response);
    }
}
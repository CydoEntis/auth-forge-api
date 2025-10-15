using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.AuthForge.Commands.Refresh;

public sealed class RefreshDeveloperTokenCommandHandler 
    : ICommandHandler<RefreshDeveloperTokenCommand, Result<RefreshDeveloperTokenResponse>>
{
    private readonly IAuthForgeUserRepository _userRepository;
    private readonly IAuthForgeRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthForgeJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshDeveloperTokenCommandHandler(
        IAuthForgeUserRepository userRepository,
        IAuthForgeRefreshTokenRepository refreshTokenRepository,
        IAuthForgeJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RefreshDeveloperTokenResponse>> Handle(
        RefreshDeveloperTokenCommand command,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (refreshToken is null)
            return Result<RefreshDeveloperTokenResponse>.Failure(AuthForgeRefreshTokenErrors.Invalid);

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
            return Result<RefreshDeveloperTokenResponse>.Failure(AuthForgeUserErrors.NotFound);

        var validationResult = ValidateRefreshToken(refreshToken, user);
        if (validationResult.IsFailure)
        {
            if (refreshToken.IsRevoked)
            {
                await RevokeAllUserTokensAsync(user.Id, cancellationToken);
            }
            return Result<RefreshDeveloperTokenResponse>.Failure(validationResult.Error);
        }

        var tokenPair = _jwtTokenGenerator.GenerateTokenPair(
            user,
            command.IpAddress,
            command.UserAgent);

        refreshToken.MarkAsUsed();
        refreshToken.Revoke(tokenPair.RefreshToken);
        _refreshTokenRepository.Update(refreshToken);

        var newRefreshToken = AuthForgeRefreshToken.Create(
            user.Id,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAt,
            command.IpAddress,
            command.UserAgent);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        await RemoveOldRefreshTokensAsync(user.Id, cancellationToken);

        user.RecordSuccessfulLogin();
        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RefreshDeveloperTokenResponse(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshTokenExpiresAt,
            tokenPair.ExpiresInSeconds);

        return Result<RefreshDeveloperTokenResponse>.Success(response);
    }

    private static Result ValidateRefreshToken(AuthForgeRefreshToken refreshToken, AuthForgeUser user)
    {
        if (refreshToken.IsExpired)
            return Result.Failure(AuthForgeRefreshTokenErrors.Expired);

        if (refreshToken.IsRevoked)
            return Result.Failure(AuthForgeRefreshTokenErrors.Revoked);

        if (!user.IsActive)
            return Result.Failure(AuthForgeUserErrors.Inactive);

        return Result.Success();
    }

    private async Task RevokeAllUserTokensAsync(AuthForgeUserId userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveTokensForUserAsync(userId, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
            _refreshTokenRepository.Update(token);
        }
    }

    private async Task RemoveOldRefreshTokensAsync(AuthForgeUserId userId, CancellationToken cancellationToken)
    {
        var allTokens = await _refreshTokenRepository.GetByUserIdAsync(userId, cancellationToken);
        var cutoffDate = DateTime.UtcNow.AddDays(-90);

        var tokensToRemove = allTokens
            .Where(t => !t.IsActive && t.CreatedAtUtc < cutoffDate)
            .ToList();

        foreach (var token in tokensToRemove)
        {
            _refreshTokenRepository.Delete(token);
        }
    }
}
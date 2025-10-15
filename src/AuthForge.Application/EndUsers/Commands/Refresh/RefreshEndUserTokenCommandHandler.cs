using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.EndUsers.Commands.Refresh;

public sealed class RefreshEndUserTokenCommandHandler 
    : ICommandHandler<RefreshEndUserTokenCommand, Result<RefreshEndUserTokenResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IEndUserRefreshTokenRepository _refreshTokenRepository;
    private readonly IEndUserJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshEndUserTokenCommandHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository,
        IEndUserRefreshTokenRepository refreshTokenRepository,
        IEndUserJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RefreshEndUserTokenResponse>> Handle(
        RefreshEndUserTokenCommand command,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (refreshToken is null)
            return Result<RefreshEndUserTokenResponse>.Failure(EndUserRefreshTokenErrors.Invalid);

        var user = await _endUserRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
            return Result<RefreshEndUserTokenResponse>.Failure(EndUserErrors.NotFound);

        var application = await _applicationRepository.GetByIdAsync(user.ApplicationId, cancellationToken);
        if (application is null)
            return Result<RefreshEndUserTokenResponse>.Failure(ApplicationErrors.NotFound);

        var validationResult = ValidateRefreshToken(refreshToken, user, application);
        if (validationResult.IsFailure)
        {
            if (refreshToken.IsRevoked)
            {
                await RevokeAllUserTokensAsync(user.Id, cancellationToken);
            }
            return Result<RefreshEndUserTokenResponse>.Failure(validationResult.Error);
        }

        var tokenPair = _jwtTokenGenerator.GenerateTokenPair(
            user,
            application,
            command.IpAddress,
            command.UserAgent);

        refreshToken.MarkAsUsed();
        refreshToken.Revoke(tokenPair.RefreshToken);
        _refreshTokenRepository.Update(refreshToken);

        var newRefreshToken = EndUserRefreshToken.Create(
            user.Id,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAt,
            command.IpAddress,
            command.UserAgent);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        await RemoveOldRefreshTokensAsync(user.Id, cancellationToken);

        user.RecordSuccessfulLogin();
        _endUserRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RefreshEndUserTokenResponse(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshTokenExpiresAt,
            tokenPair.ExpiresInSeconds);

        return Result<RefreshEndUserTokenResponse>.Success(response);
    }

    private static Result ValidateRefreshToken(
        EndUserRefreshToken refreshToken,
        EndUser user,
        App application)
    {
        if (refreshToken.IsExpired)
            return Result.Failure(EndUserRefreshTokenErrors.Expired);

        if (refreshToken.IsRevoked)
            return Result.Failure(EndUserRefreshTokenErrors.Revoked);

        if (!user.IsActive)
            return Result.Failure(EndUserErrors.Inactive);

        if (!application.IsActive)
            return Result.Failure(ApplicationErrors.Inactive);

        return Result.Success();
    }

    private async Task RevokeAllUserTokensAsync(EndUserId userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveTokensForUserAsync(userId, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
            _refreshTokenRepository.Update(token);
        }
    }

    private async Task RemoveOldRefreshTokensAsync(EndUserId userId, CancellationToken cancellationToken)
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
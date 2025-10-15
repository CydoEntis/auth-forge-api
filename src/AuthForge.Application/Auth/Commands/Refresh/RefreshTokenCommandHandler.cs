using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IEndUserTokenGenerator _endUserTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITenantRepository tenantRepository,
        IEndUserTokenGenerator endUserTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tenantRepository = tenantRepository;
        _endUserTokenGenerator = endUserTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (refreshToken is null)
            return Result<RefreshTokenResponse>.Failure(
                DomainErrors.RefreshToken.Invalid);

        var user = await _userRepository.GetByIdAsync(
            refreshToken.UserId,
            cancellationToken);

        if (user is null)
            return Result<RefreshTokenResponse>.Failure(
                DomainErrors.User.NotFound);

        var validationResult = ValidateRefreshToken(refreshToken, user);
        if (validationResult.IsFailure)
        {
            if (refreshToken.IsRevoked)
            {
                await RevokeAllUserTokensAsync(user.Id, cancellationToken);
            }

            return Result<RefreshTokenResponse>.Failure(validationResult.Error);
        }

        var tenant = await _tenantRepository.GetByIdAsync(
            user.TenantId,
            cancellationToken);

        if (tenant is null)
            return Result<RefreshTokenResponse>.Failure(
                DomainErrors.Tenant.NotFound);

        var tokenPair = _endUserTokenGenerator.GenerateTokenPair(
            user,
            tenant,
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
        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSuccessResponse(tokenPair);
    }

    private static Result ValidateRefreshToken(
        EndUserRefreshToken endUserRefreshToken,
        User user)
    {
        if (endUserRefreshToken.IsExpired)
            return Result.Failure(DomainErrors.RefreshToken.Expired);

        if (endUserRefreshToken.IsRevoked)
            return Result.Failure(DomainErrors.RefreshToken.Revoked);

        if (!user.IsActive)
            return Result.Failure(DomainErrors.User.Inactive);

        return Result.Success();
    }

    private async Task RevokeAllUserTokensAsync(UserId userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveTokensForUserAsync(
            userId,
            cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
            _refreshTokenRepository.Update(token);
        }
    }

    private async Task RemoveOldRefreshTokensAsync(UserId userId, CancellationToken cancellationToken)
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

    private static Result<RefreshTokenResponse> CreateSuccessResponse(
        TokenPair tokenPair)
    {
        var response = new RefreshTokenResponse(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshTokenExpiresAt,
            tokenPair.ExpiresInSeconds);

        return Result<RefreshTokenResponse>.Success(response);
    }
}
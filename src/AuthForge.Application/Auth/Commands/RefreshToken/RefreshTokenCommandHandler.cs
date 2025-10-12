using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var userResult = await GetUserByRefreshTokenAsync(
            command.RefreshToken,
            cancellationToken);
        if (userResult.IsFailure)
            return Result<RefreshTokenResponse>.Failure(userResult.Error);

        var user = userResult.Value;

        var refreshToken = user.RefreshTokens
            .FirstOrDefault(rt => rt.Token == command.RefreshToken);

        if (refreshToken is null)
            return Result<RefreshTokenResponse>.Failure(
                DomainErrors.RefreshToken.Invalid);

        var validationResult = ValidateRefreshToken(refreshToken, user);
        if (validationResult.IsFailure)
        {
            if (refreshToken.IsRevoked)
            {
                user.RevokeAllRefreshTokens();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result<RefreshTokenResponse>.Failure(validationResult.Error);
        }

        var tenant = await _tenantRepository.GetByIdAsync(
            user.TenantId,
            cancellationToken);

        if (tenant is null)
            return Result<RefreshTokenResponse>.Failure(
                DomainErrors.Tenant.NotFound);

        var tokenPair = _jwtTokenGenerator.GenerateTokenPair(
            user,
            tenant,
            command.IpAddress,
            command.UserAgent);

        refreshToken.MarkAsUsed();
        refreshToken.Revoke(tokenPair.RefreshToken);

        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAt,
            command.IpAddress,
            command.UserAgent);

        user.AddRefreshToken(newRefreshToken);

        user.RemoveOldRefreshTokens(90);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSuccessResponse(tokenPair);
    }

    private async Task<Result<User>> GetUserByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(
            refreshToken,
            cancellationToken);

        if (user is null)
            return Result<User>.Failure(DomainErrors.RefreshToken.Invalid);

        return Result<User>.Success(user);
    }

    private static Result ValidateRefreshToken(
        Domain.Entities.RefreshToken refreshToken,
        User user)
    {
        if (refreshToken.IsExpired)
            return Result.Failure(DomainErrors.RefreshToken.Expired);

        if (refreshToken.IsRevoked)
            return Result.Failure(DomainErrors.RefreshToken.Revoked);

        if (!user.IsActive)
            return Result.Failure(DomainErrors.User.Inactive);

        return Result.Success();
    }

    private static Result<RefreshTokenResponse> CreateSuccessResponse(
        Domain.ValueObjects.TokenPair tokenPair)
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
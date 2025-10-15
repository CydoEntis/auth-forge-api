using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.AuthForge.Commands.Login;

public sealed class LoginDeveloperCommandHandler
    : ICommandHandler<LoginDeveloperCommand, Result<LoginDeveloperResponse>>
{
    private readonly IAuthForgeUserRepository _userRepository;
    private readonly IAuthForgeRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthForgeJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailParser _emailParser;
    private readonly IUnitOfWork _unitOfWork;

    public LoginDeveloperCommandHandler(
        IAuthForgeUserRepository userRepository,
        IAuthForgeRefreshTokenRepository refreshTokenRepository,
        IAuthForgeJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IEmailParser emailParser,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _emailParser = emailParser;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<LoginDeveloperResponse>> Handle(
        LoginDeveloperCommand command,
        CancellationToken cancellationToken)
    {
        var emailResult = _emailParser.ParseForAuthentication(command.Email);
        if (emailResult.IsFailure)
            return Result<LoginDeveloperResponse>.Failure(emailResult.Error);

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
            return Result<LoginDeveloperResponse>.Failure(AuthForgeUserErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result<LoginDeveloperResponse>.Failure(AuthForgeUserErrors.Inactive);

        if (!_passwordHasher.VerifyPassword(command.Password, user.HashedPassword))
            return Result<LoginDeveloperResponse>.Failure(AuthForgeUserErrors.InvalidCredentials);

        var tokenPair = _jwtTokenGenerator.GenerateTokenPair(
            user,
            command.IpAddress,
            command.UserAgent);

        await StoreRefreshTokenAsync(
            user.Id,
            tokenPair,
            command.IpAddress,
            command.UserAgent,
            cancellationToken);

        user.RecordSuccessfulLogin();

        await RemoveOldRefreshTokensAsync(user.Id, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginDeveloperResponse(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FullName,
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshTokenExpiresAt,
            tokenPair.ExpiresInSeconds);

        return Result<LoginDeveloperResponse>.Success(response);
    }

    private async Task StoreRefreshTokenAsync(
        AuthForgeUserId userId,
        TokenPair tokenPair,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var refreshToken = AuthForgeRefreshToken.Create(
            userId,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAt,
            ipAddress,
            userAgent);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
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
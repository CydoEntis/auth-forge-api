using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantValidationService _tenantValidationService;
    private readonly IEmailParser _emailParser;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        ITenantValidationService tenantValidationService,
        IEmailParser emailParser,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _tenantValidationService = tenantValidationService;
        _emailParser = emailParser;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async ValueTask<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var tenantResult = await _tenantValidationService.ValidateTenantAsync(
            command.TenantId,
            cancellationToken);
        if (tenantResult.IsFailure)
            return Result<LoginResponse>.Failure(tenantResult.Error);

        var emailResult = _emailParser.ParseForAuthentication(command.Email);
        if (emailResult.IsFailure)
            return Result<LoginResponse>.Failure(emailResult.Error);

        var userResult = await ValidateUserAsync(
            tenantResult.Value,
            emailResult.Value,
            cancellationToken);
        if (userResult.IsFailure)
            return Result<LoginResponse>.Failure(userResult.Error);

        var passwordResult = await VerifyPasswordAsync(
            userResult.Value,
            command.Password,
            tenantResult.Value,
            cancellationToken);
        if (passwordResult.IsFailure)
            return Result<LoginResponse>.Failure(passwordResult.Error);

        var tokenPair = _jwtTokenGenerator.GenerateTokenPair(
            userResult.Value,
            tenantResult.Value,
            command.IpAddress,
            command.UserAgent);

        await StoreRefreshTokenAsync(
            userResult.Value.Id,
            tokenPair,
            command.IpAddress,
            command.UserAgent,
            cancellationToken);

        userResult.Value.RecordSuccessfulLogin();

        await RemoveOldRefreshTokensAsync(userResult.Value.Id, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSuccessResponse(userResult.Value, tokenPair);
    }

    private async Task<Result<User>> ValidateUserAsync(
        Tenant tenant,
        Email email,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(tenant.Id, email, cancellationToken);

        if (user is null)
            return Result<User>.Failure(DomainErrors.User.InvalidCredentials);

        if (user.IsLockedOut())
            return Result<User>.Failure(DomainErrors.User.LockedOutUntil(user.LockedOutUntil!.Value));

        if (!user.IsActive)
            return Result<User>.Failure(DomainErrors.User.InvalidCredentials);

        return Result<User>.Success(user);
    }

    private async Task<Result> VerifyPasswordAsync(
        User user,
        string password,
        Tenant tenant,
        CancellationToken cancellationToken)
    {
        if (!user.PasswordHash.Verify(password))
        {
            user.RecordFailedLogin(
                tenant.Settings.MaxFailedLoginAttempts,
                tenant.Settings.LockoutDurationMinutes);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure(DomainErrors.User.InvalidCredentials);
        }

        return Result.Success();
    }

    private async Task StoreRefreshTokenAsync(
        UserId userId,
        TokenPair tokenPair,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var refreshToken = RefreshToken.Create(
            userId,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAt,
            ipAddress,
            userAgent);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
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

    private static Result<LoginResponse> CreateSuccessResponse(User user, TokenPair tokenPair)
    {
        var response = new LoginResponse(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FullName,
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshTokenExpiresAt,
            tokenPair.ExpiresInSeconds);

        return Result<LoginResponse>.Success(response);
    }
}
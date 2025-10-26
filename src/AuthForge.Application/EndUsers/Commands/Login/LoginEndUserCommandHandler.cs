using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.EndUsers.Commands.Login;

public sealed class LoginEndUserCommandHandler
    : ICommandHandler<LoginEndUserCommand, Result<LoginEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IEndUserRefreshTokenRepository _refreshTokenRepository;
    private readonly IEndUserJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailParser _emailParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginEndUserCommandHandler> _logger;

    public LoginEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository,
        IEndUserRefreshTokenRepository refreshTokenRepository,
        IEndUserJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IEmailParser emailParser,
        IUnitOfWork unitOfWork,
        ILogger<LoginEndUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _emailParser = emailParser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<LoginEndUserResponse>> Handle(
        LoginEndUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for email {Email} from IP {IpAddress}", command.Email, command.IpAddress);

        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
        {
            _logger.LogWarning("Login failed - Invalid ApplicationId format: {ApplicationId}", command.ApplicationId);
            return Result<LoginEndUserResponse>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));
        }

        var applicationId = ApplicationId.Create(appGuid);
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            _logger.LogWarning("Login failed - Application not found: {ApplicationId}", applicationId);
            return Result<LoginEndUserResponse>.Failure(ApplicationErrors.NotFound);
        }

        if (!application.IsActive)
        {
            _logger.LogWarning("Login failed - Application inactive: {ApplicationName} ({ApplicationId})",
                application.Name, applicationId);
            return Result<LoginEndUserResponse>.Failure(ApplicationErrors.Inactive);
        }

        var emailResult = _emailParser.ParseForAuthentication(command.Email);
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Login failed - Invalid email format: {Email}", command.Email);
            return Result<LoginEndUserResponse>.Failure(emailResult.Error);
        }

        var user = await _endUserRepository.GetByEmailAsync(
            applicationId,
            emailResult.Value,
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Login failed - User not found for email {Email} in application {ApplicationName}",
                command.Email, application.Name);
            return Result<LoginEndUserResponse>.Failure(EndUserErrors.InvalidCredentials);
        }

        if (user.IsLockedOut())
        {
            _logger.LogWarning("Login failed - User {UserId} is locked out until {LockedOutUntil}",
                user.Id, user.LockedOutUntil!.Value);
            return Result<LoginEndUserResponse>.Failure(EndUserErrors.LockedOutUntil(user.LockedOutUntil!.Value));
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed - User {UserId} is inactive", user.Id);
            return Result<LoginEndUserResponse>.Failure(EndUserErrors.Inactive);
        }

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - Invalid password for user {UserId}. Failed attempts: {FailedAttempts}",
                user.Id, user.FailedLoginAttempts + 1);

            user.RecordFailedLogin(
                application.Settings.MaxFailedLoginAttempts,
                application.Settings.LockoutDurationMinutes);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<LoginEndUserResponse>.Failure(EndUserErrors.InvalidCredentials);
        }

        var tokenPair = _jwtTokenGenerator.GenerateTokenPair(
            user,
            application,
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

        _logger.LogInformation("User {UserId} ({Email}) successfully logged in from IP {IpAddress}",
            user.Id, user.Email.Value, command.IpAddress);

        var response = new LoginEndUserResponse(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FullName,
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshTokenExpiresAt,
            tokenPair.ExpiresInSeconds);

        return Result<LoginEndUserResponse>.Success(response);
    }

    private async Task StoreRefreshTokenAsync(
        EndUserId userId,
        TokenPair tokenPair,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var refreshToken = EndUserRefreshToken.Create(
            userId,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAt,
            ipAddress,
            userAgent);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
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
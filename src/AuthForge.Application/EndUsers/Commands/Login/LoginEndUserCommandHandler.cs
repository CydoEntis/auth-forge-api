using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
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

    public LoginEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository,
        IEndUserRefreshTokenRepository refreshTokenRepository,
        IEndUserJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IEmailParser emailParser,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _emailParser = emailParser;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<LoginEndUserResponse>> Handle(
        LoginEndUserCommand command,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
            return Result<LoginEndUserResponse>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));

        var applicationId = ApplicationId.Create(appGuid);
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
            return Result<LoginEndUserResponse>.Failure(ApplicationErrors.NotFound);

        if (!application.IsActive)
            return Result<LoginEndUserResponse>.Failure(ApplicationErrors.Inactive);

        var emailResult = _emailParser.ParseForAuthentication(command.Email);
        if (emailResult.IsFailure)
            return Result<LoginEndUserResponse>.Failure(emailResult.Error);

        var user = await _endUserRepository.GetByEmailAsync(
            applicationId,
            emailResult.Value,
            cancellationToken);

        if (user is null)
            return Result<LoginEndUserResponse>.Failure(EndUserErrors.InvalidCredentials);

        if (user.IsLockedOut())
            return Result<LoginEndUserResponse>.Failure(EndUserErrors.LockedOutUntil(user.LockedOutUntil!.Value));

        if (!user.IsActive)
            return Result<LoginEndUserResponse>.Failure(EndUserErrors.Inactive);

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
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
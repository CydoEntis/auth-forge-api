// src/AuthForge.Application/Admin/Commands/Login/LoginAdminCommandHandler.cs

using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Settings;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Options;

namespace AuthForge.Application.Admin.Commands.Login;

public sealed class LoginAdminCommandHandler
    : ICommandHandler<LoginAdminCommand, Result<LoginAdminResponse>>
{
    private readonly IOptions<AuthForgeSettings> _settings;
    private readonly IAdminJwtTokenGenerator _tokenGenerator;
    private readonly IAdminRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginAdminCommandHandler(
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

    public async ValueTask<Result<LoginAdminResponse>> Handle(
        LoginAdminCommand command,
        CancellationToken cancellationToken)
    {
        var adminSettings = _settings.Value.Admin;

        if (command.Email != adminSettings.Email)
        {
            return Result<LoginAdminResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        var configPasswordHash = HashedPassword.Create(adminSettings.Password);
        
        if (!configPasswordHash.Verify(command.Password))
        {
            return Result<LoginAdminResponse>.Failure(AdminErrors.InvalidCredentials);
        }

        var accessToken = _tokenGenerator.GenerateAccessToken(command.Email);
        var refreshTokenString = _tokenGenerator.GenerateRefreshToken();

        var refreshTokenExpiresAt = DateTime.UtcNow
            .AddDays(_settings.Value.Jwt.RefreshTokenExpirationDays);

        var refreshToken = AdminRefreshToken.Create(
            refreshTokenString,
            refreshTokenExpiresAt);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginAdminResponse(
            accessToken,
            refreshTokenString,
            DateTime.UtcNow.AddMinutes(_settings.Value.Jwt.AccessTokenExpirationMinutes));

        return Result<LoginAdminResponse>.Success(response);
    }
}
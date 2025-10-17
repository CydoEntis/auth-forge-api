using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Settings;
using AuthForge.Domain.Common;
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

    public LoginAdminCommandHandler(
        IOptions<AuthForgeSettings> settings,
        IAdminJwtTokenGenerator tokenGenerator)
    {
        _settings = settings;
        _tokenGenerator = tokenGenerator;
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
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        var response = new LoginAdminResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_settings.Value.Jwt.AccessTokenExpirationMinutes));

        return Result<LoginAdminResponse>.Success(response);
    }
}

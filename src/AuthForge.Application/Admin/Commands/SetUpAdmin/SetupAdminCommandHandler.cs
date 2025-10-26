using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Settings;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthForge.Application.Admin.Commands.SetUpAdmin;

public sealed class SetupAdminCommandHandler
    : ICommandHandler<SetupAdminCommand, Result<SetupAdminResponse>>
{
    private readonly IAdminRepository _adminRepository;
    private readonly IAdminRefreshTokenRepository _adminRefreshTokenRepository;
    private readonly IAdminJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuthForgeSettings _settings;
    private readonly ILogger<SetupAdminCommandHandler> _logger;

    public SetupAdminCommandHandler(
        IAdminRepository adminRepository,
        IAdminRefreshTokenRepository adminRefreshTokenRepository,
        IAdminJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<AuthForgeSettings> settings,
        ILogger<SetupAdminCommandHandler> logger)
    {
        _adminRepository = adminRepository;
        _adminRefreshTokenRepository = adminRefreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _logger = logger;
    }

    public async ValueTask<Result<SetupAdminResponse>> Handle(
        SetupAdminCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin setup attempt for email {Email}", command.Email);

        var adminExists = await _adminRepository.AnyExistsAsync(cancellationToken);
        if (adminExists)
        {
            _logger.LogWarning("Admin setup failed - Admin already exists");
            return Result<SetupAdminResponse>.Failure(AdminErrors.AlreadyExists);
        }

        var hashedPassword = HashedPassword.Create(command.Password);
        var admin = Domain.Entities.Admin.Create(
            Email.Create(command.Email),
            hashedPassword);

        await _adminRepository.AddAsync(admin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(admin.Email.Value);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_settings.Jwt.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var tokens = new TokenPair(
            accessToken,
            refreshTokenString,
            accessTokenExpiresAt,
            refreshTokenExpiresAt);

        var refreshToken = AdminRefreshToken.Create(
            admin.Id,
            refreshTokenString,
            refreshTokenExpiresAt);

        await _adminRefreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin account successfully created: {AdminId} ({Email})", admin.Id, admin.Email.Value);

        var adminDetails = new AdminDetails(
            admin.Id.Value,
            admin.Email.Value,
            admin.CreatedAtUtc);

        var response = new SetupAdminResponse(
            "Admin account created successfully. You can now manage your applications.",
            tokens,
            adminDetails);

        return Result<SetupAdminResponse>.Success(response);
    }
}
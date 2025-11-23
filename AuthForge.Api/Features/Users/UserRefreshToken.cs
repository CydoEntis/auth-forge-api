using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserRefreshTokenRequest(string RefreshToken);

public sealed record UserRefreshTokenResponse(TokenPair Tokens);

public sealed class UserRefreshTokenValidator : AbstractValidator<UserRefreshTokenRequest>
{
    public UserRefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}

public sealed class UserRefreshTokenHandler
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserRefreshTokenHandler> _logger;

    public UserRefreshTokenHandler(
        AppDbContext context,
        IJwtService jwtService,
        ILogger<UserRefreshTokenHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<UserRefreshTokenResponse> HandleAsync(
        Guid applicationId,
        UserRefreshTokenRequest request,
        CancellationToken ct)
    {
        var app = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDeleted, ct);

        if (app == null || !app.IsActive)
        {
            _logger.LogWarning("Refresh attempt for invalid application: {AppId}", applicationId);
            throw new UnauthorizedException("Invalid refresh token");
        }

        var storedToken = await _context.UserRefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == request.RefreshToken
                && rt.User.ApplicationId == applicationId, ct);

        if (storedToken == null)
        {
            _logger.LogWarning("Invalid refresh token for app {AppId}", applicationId);
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token expired. Please log in again.");
        }

        if (storedToken.IsRevoked)
        {
            throw new UnauthorizedException("Refresh token revoked. Please log in again.");
        }

        var user = storedToken.User;
        if (user.ApplicationId != applicationId)
        {
            _logger.LogError(
                "Token application mismatch. Token: {TokenApp}, Request: {RequestApp}",
                user.ApplicationId,
                applicationId);
            throw new UnauthorizedException("Invalid refresh token");
        }

        var newTokens = await _jwtService.GenerateUserTokenPairAsync(
            user.Id,
            user.Email,
            app.Id,
            app.JwtSecretEncrypted,
            app.AccessTokenExpirationMinutes,
            app.RefreshTokenExpirationDays);

        storedToken.IsRevoked = true;

        var newRefreshToken = new Entities.UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newTokens.RefreshToken,
            ExpiresAt = newTokens.RefreshTokenExpiresAt,
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.UserRefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Refresh token rotated for user {UserId} in app {AppId}",
            user.Id,
            applicationId);

        return new UserRefreshTokenResponse(Tokens: newTokens);
    }
}

public static class UserRefreshToken
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/refresh", async (
                Guid appId,
                UserRefreshTokenRequest request,
                UserRefreshTokenHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UserRefreshTokenValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new FluentValidation.ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserRefreshTokenResponse>.Ok(response));
            })
            .WithName("UserRefreshToken")
            .WithTags("User Auth")
            .AllowAnonymous();
    }
}
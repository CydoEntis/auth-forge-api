using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Auth;

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record RefreshTokenResponse(TokenPair Tokens);

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenHandler
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        AppDbContext context,
        IJwtService jwtService,
        ILogger<RefreshTokenHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> HandleAsync(RefreshTokenRequest request, CancellationToken ct)
    {
        var storedToken = await _context.AdminRefreshTokens
            .Include(rt => rt.Admin)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (storedToken == null)
        {
            _logger.LogWarning("Invalid refresh token attempt");
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired refresh token used: {TokenId}", storedToken.Id);
            throw new UnauthorizedException("Refresh token expired.");
        }

        if (storedToken.IsRevoked)
        {
            _logger.LogWarning("Revoked refresh token used: {TokenId}", storedToken.Id);
            throw new UnauthorizedException("Refresh token has been revoked.");
        }

        var newTokens = await _jwtService.GenerateAdminTokenPairAsync(
            storedToken.AdminId,
            storedToken.Admin.Email);

        storedToken.IsRevoked = true;

        var newRefreshToken = new Entities.AdminRefreshToken
        {
            Id = Guid.NewGuid(),
            AdminId = storedToken.AdminId,
            Token = newTokens.RefreshToken,
            ExpiresAt = newTokens.RefreshTokenExpiresAt,
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.AdminRefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Token refreshed for account {AdminId}", storedToken.AdminId);

        return new RefreshTokenResponse(newTokens);
    }
}

public static class RefreshToken
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/refresh", async (
                RefreshTokenRequest request,
                RefreshTokenHandler handler,
                CancellationToken ct) =>
            {
                var validator = new RefreshTokenValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<RefreshTokenResponse>.Ok(response));
            })
            .WithName("RefreshToken")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
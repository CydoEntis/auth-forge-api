using AuthForge.Api.Common;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminRegenerateJwtSecretResponse(string Message);

public class AdminRegenerateJwtSecretHandler
{
    private readonly ConfigDbContext _configDb;
    private readonly AppDbContext _appDb;
    private readonly IEncryptionService _encryptionService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AdminRegenerateJwtSecretHandler> _logger;

    public AdminRegenerateJwtSecretHandler(
        ConfigDbContext configDb,
        AppDbContext appDb,
        IEncryptionService encryptionService,
        IJwtService jwtService,
        ILogger<AdminRegenerateJwtSecretHandler> logger)
    {
        _configDb = configDb;
        _appDb = appDb;
        _encryptionService = encryptionService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AdminRegenerateJwtSecretResponse> HandleAsync(CancellationToken ct)
    {
        var newSecret = _jwtService.GenerateSecureToken(64);

        var config = await _configDb.Configuration.FirstAsync(ct);
        config.JwtSecretEncrypted = _encryptionService.Encrypt(newSecret);
        config.UpdatedAtUtc = DateTime.UtcNow;

        var allRefreshTokens = await _appDb.AdminRefreshTokens
            .Where(rt => !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in allRefreshTokens)
        {
            token.IsRevoked = true;
        }

        await _configDb.SaveChangesAsync(ct);
        await _appDb.SaveChangesAsync(ct);

        _logger.LogWarning("JWT secret regenerated - all admin sessions revoked");

        return new AdminRegenerateJwtSecretResponse(
            "JWT secret regenerated successfully. All admins must log in again.");
    }
}

public static class AdminRegenerateJwtSecret
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/jwt/regenerate", async (
                AdminRegenerateJwtSecretHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<AdminRegenerateJwtSecretResponse>.Ok(response));
            })
            .WithName("AdminRegenerateJwtSecret")
            .WithTags("Admin")
            .RequireAuthorization();
    }
}
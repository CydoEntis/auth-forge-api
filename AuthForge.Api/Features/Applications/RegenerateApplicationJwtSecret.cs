using System.Security.Cryptography;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record RegenerateApplicationJwtSecretResponse(
    Guid Id,
    string Name,
    DateTime RegeneratedAtUtc,
    string Warning
);

public class RegenerateApplicationJwtSecretHandler
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<RegenerateApplicationJwtSecretHandler> _logger;

    public RegenerateApplicationJwtSecretHandler(
        AppDbContext context,
        IEncryptionService encryptionService,
        ILogger<RegenerateApplicationJwtSecretHandler> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<RegenerateApplicationJwtSecretResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications
            .Include(a => a.Users)
            .ThenInclude(u => u.RefreshTokens)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application with ID {id} not found");
        }

        var newJwtSecret = GenerateJwtSecret();
        var regeneratedAt = DateTime.UtcNow;

        application.JwtSecretEncrypted = _encryptionService.Encrypt(newJwtSecret);
        application.UpdatedAtUtc = regeneratedAt;

        foreach (var user in application.Users)
        {
            foreach (var token in user.RefreshTokens.Where(t => !t.IsRevoked))
            {
                token.IsRevoked = true;
            }
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "JWT secret regenerated for application: {Name} ({Id}) - all user sessions invalidated",
            application.Name, id);

        return new RegenerateApplicationJwtSecretResponse(
            id,
            application.Name,
            regeneratedAt,
            "JWT secret regenerated. All existing user sessions have been invalidated. Users will need to log in again"
        );
    }

    private static string GenerateJwtSecret()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

public static class RegenerateJwtSecretFeature
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/applications/{{id:guid}}/regenerate-jwt-secret", async (
                Guid id,
                [FromServices] RegenerateApplicationJwtSecretHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(id, ct);
                return Results.Ok(ApiResponse<RegenerateApplicationJwtSecretResponse>.Ok(response));
            })
            .WithName("RegenerateJwtSecret")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}
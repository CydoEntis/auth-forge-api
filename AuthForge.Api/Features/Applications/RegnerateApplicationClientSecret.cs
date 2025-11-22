using System.Security.Cryptography;
using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record RegenerateApplicationClientSecretResponse(
    Guid Id,
    string Name,
    string ClientId,
    string NewClientSecret,
    DateTime RegeneratedAtUtc,
    string Warning
);

public class RegenerateApplicationClientSecretHandler
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<RegenerateApplicationClientSecretHandler> _logger;

    public RegenerateApplicationClientSecretHandler(
        AppDbContext context,
        IEncryptionService encryptionService,
        ILogger<RegenerateApplicationClientSecretHandler> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<RegenerateApplicationClientSecretResponse> HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application with ID {id} not found");
        }

        var newClientSecret = GenerateClientSecret();
        var regeneratedAt = DateTime.UtcNow;

        application.ClientSecretEncrypted = _encryptionService.Encrypt(newClientSecret);
        application.UpdatedAtUtc = regeneratedAt;

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Client secret regenerated for application: {Name} ({Id})",
            application.Name, id);

        return new RegenerateApplicationClientSecretResponse(
            id,
            application.Name,
            application.ClientId,
            newClientSecret,
            regeneratedAt,
            "Your old client secret will no longer work. Update your application immediately"
        );
    }

    private static string GenerateClientSecret()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}

public static class RegenerateApplicationClientSecret
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/applications/{{id:guid}}/regenerate-secret", async (
                Guid id,
                RegenerateApplicationClientSecretHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(id, ct);
                return Results.Ok(ApiResponse<RegenerateApplicationClientSecretResponse>.Ok(response));
            })
            .WithName("RegenerateApplicationClientSecret")
            .WithTags("Applications")
            .RequireAuthorization();
    }
}
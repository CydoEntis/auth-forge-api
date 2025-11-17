using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminRevokeAllSessionsResponse(string Message, int SessionsRevoked);

public class AdminRevokeAllSessionsHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AdminRevokeAllSessionsHandler> _logger;

    public AdminRevokeAllSessionsHandler(AppDbContext context, ICurrentUserService currentUser, ILogger<AdminRevokeAllSessionsHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AdminRevokeAllSessionsResponse> HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var adminId))
            throw new UnauthorizedException("Invalid user ID");

        var tokens = await _context.AdminRefreshTokens
            .Where(rt => rt.AdminId == adminId && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning("Admin {AdminId} revoked all sessions ({Count} tokens)", adminId, tokens.Count);

        return new AdminRevokeAllSessionsResponse(
            "All sessions revoked. You will need to log in again.",
            tokens.Count);
    }
}

public static class AdminRevokeAllSessions
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/sessions/revoke-all", async (
                AdminRevokeAllSessionsHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<AdminRevokeAllSessionsResponse>.Ok(response));
            })
            .WithName("AdminRevokeAllSessions")
            .WithTags("Admin")
            .RequireAuthorization();
    }
}
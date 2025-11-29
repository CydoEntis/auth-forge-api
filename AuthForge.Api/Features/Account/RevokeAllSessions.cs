using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Account;

public sealed record RevokeAllSessionsResponse(string Message, int SessionsRevoked);

public class RevokeAllSessionsHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RevokeAllSessionsHandler> _logger;

    public RevokeAllSessionsHandler(AppDbContext context, ICurrentUserService currentUser, ILogger<RevokeAllSessionsHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<RevokeAllSessionsResponse> HandleAsync(CancellationToken ct)
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

        _logger.LogWarning("Account {AdminId} revoked all sessions ({Count} tokens)", adminId, tokens.Count);

        return new RevokeAllSessionsResponse(
            "All sessions revoked. You will need to log in again.",
            tokens.Count);
    }
}

public static class RevokeAllSessions
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/sessions/revoke-all", async (
                RevokeAllSessionsHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(ct);
                return Results.Ok(ApiResponse<RevokeAllSessionsResponse>.Ok(response));
            })
            .WithName("RevokeAllSessions")
            .WithTags("Account")
            .RequireAuthorization();
    }
}
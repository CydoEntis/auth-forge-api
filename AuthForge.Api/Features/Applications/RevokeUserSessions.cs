using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record RevokeUserSessionsResponse(
    Guid UserId,
    string Email,
    int SessionsRevoked,
    string Message
);

public sealed class RevokeUserSessionsHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RevokeUserSessionsHandler> _logger;

    public RevokeUserSessionsHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<RevokeUserSessionsHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<RevokeUserSessionsResponse> HandleAsync(
        Guid applicationId,
        Guid userId,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application {applicationId} not found");
        }

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId && u.ApplicationId == applicationId, ct);

        if (user == null)
        {
            throw new NotFoundException($"User {userId} not found in application {applicationId}");
        }

        var activeTokens = user.RefreshTokens.Where(rt => !rt.IsRevoked).ToList();
        var revokedCount = activeTokens.Count;

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Admin {AdminId} REVOKED {Count} sessions for user {UserId} ({Email}) in application {AppId}",
            _currentUser.UserId,
            revokedCount,
            userId,
            user.Email,
            applicationId);

        return new RevokeUserSessionsResponse(
            userId,
            user.Email,
            revokedCount,
            $"Revoked {revokedCount} active session(s) for {user.Email}. User must log in again."
        );
    }
}

public static class RevokeUserSessions
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/applications/{{appId:guid}}/users/{{userId:guid}}/revoke-sessions", async (
                Guid appId,
                Guid userId,
                RevokeUserSessionsHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(appId, userId, ct);
                return Results.Ok(ApiResponse<RevokeUserSessionsResponse>.Ok(response));
            })
            .WithName("RevokeUserSessions")
            .WithTags("User Management")
            .RequireAuthorization("Admin");
    }
}
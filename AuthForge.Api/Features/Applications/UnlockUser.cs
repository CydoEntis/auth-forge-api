using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record UnlockUserResponse(
    Guid Id,
    string Email,
    string Message
);

public sealed class UnlockUserHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UnlockUserHandler> _logger;

    public UnlockUserHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<UnlockUserHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<UnlockUserResponse> HandleAsync(
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
            .FirstOrDefaultAsync(u => u.Id == userId && u.ApplicationId == applicationId, ct);

        if (user == null)
        {
            throw new NotFoundException($"User {userId} not found in application {applicationId}");
        }

        user.LockedOutUntil = null;
        user.FailedLoginAttempts = 0; 
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Admin {AdminId} UNLOCKED user {UserId} ({Email}) in application {AppId}",
            _currentUser.UserId,
            userId,
            user.Email,
            applicationId);

        return new UnlockUserResponse(
            userId,
            user.Email,
            $"User {user.Email} has been unlocked and can now log in"
        );
    }
}

public static class UnlockUser
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/applications/{{appId:guid}}/users/{{userId:guid}}/unlock", async (
                Guid appId,
                Guid userId,
                UnlockUserHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(appId, userId, ct);
                return Results.Ok(ApiResponse<UnlockUserResponse>.Ok(response));
            })
            .WithName("UnlockUser")
            .WithTags("User Management")
            .RequireAuthorization("Admin");
    }
}
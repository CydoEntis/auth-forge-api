using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record GetUserResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified,
    int FailedLoginAttempts,
    bool IsLockedOut,
    DateTime? LockedOutUntil,
    DateTime? LastLoginAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    int ActiveSessionCount
);

public sealed class GetUserHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetUserHandler> _logger;

    public GetUserHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<GetUserHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<GetUserResponse> HandleAsync(
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
            .Where(u => u.Id == userId && u.ApplicationId == applicationId)
            .Select(u => new
            {
                User = u,
                ActiveSessionCount = u.RefreshTokens.Count(rt =>
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow)
            })
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            throw new NotFoundException($"User {userId} not found in application {applicationId}");
        }

        _logger.LogInformation(
            "Admin {AdminId} retrieved user {UserId} details for application {AppId}",
            _currentUser.UserId,
            userId,
            applicationId);

        return new GetUserResponse(
            user.User.Id,
            user.User.Email,
            user.User.FirstName,
            user.User.LastName,
            user.User.EmailVerified,
            user.User.FailedLoginAttempts,
            user.User.LockedOutUntil.HasValue && user.User.LockedOutUntil > DateTime.UtcNow,
            user.User.LockedOutUntil,
            user.User.LastLoginAtUtc,
            user.User.CreatedAtUtc,
            user.User.UpdatedAtUtc,
            user.ActiveSessionCount
        );
    }
}

public static class GetUser
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/applications/{{appId:guid}}/users/{{userId:guid}}", async (
                Guid appId,
                Guid userId,
                GetUserHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(appId, userId, ct);
                return Results.Ok(ApiResponse<GetUserResponse>.Ok(response));
            })
            .WithName("GetUser")
            .WithTags("User Management")
            .RequireAuthorization("Admin");
    }
}
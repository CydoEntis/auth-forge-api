using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record DeleteUserResponse(
    Guid Id,
    string Email,
    string Message
);

public sealed class DeleteUserHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteUserHandler> _logger;

    public DeleteUserHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<DeleteUserHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<DeleteUserResponse> HandleAsync(
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
            .Include(u => u.PasswordResetTokens)
            .FirstOrDefaultAsync(u => u.Id == userId && u.ApplicationId == applicationId, ct);

        if (user == null)
        {
            throw new NotFoundException($"User {userId} not found in application {applicationId}");
        }

        var userEmail = user.Email;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Admin {AdminId} DELETED user {UserId} ({Email}) from application {AppId}",
            _currentUser.UserId,
            userId,
            userEmail,
            applicationId);

        return new DeleteUserResponse(
            userId,
            userEmail,
            $"User {userEmail} has been permanently deleted"
        );
    }
}

public static class DeleteUser
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapDelete($"{prefix}/applications/{{appId:guid}}/users/{{userId:guid}}", async (
                Guid appId,
                Guid userId,
                DeleteUserHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(appId, userId, ct);
                return Results.Ok(ApiResponse<DeleteUserResponse>.Ok(response));
            })
            .WithName("DeleteUser")
            .WithTags("User Management")
            .RequireAuthorization("Admin");
    }
}
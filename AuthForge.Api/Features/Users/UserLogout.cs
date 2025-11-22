using AuthForge.Api.Common;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Users;

public sealed record UserLogoutRequest(string RefreshToken);

public sealed record UserLogoutResponse(string Message);

public sealed class UserLogoutHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserLogoutHandler> _logger;

    public UserLogoutHandler(AppDbContext context, ILogger<UserLogoutHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserLogoutResponse> HandleAsync(
        Guid applicationId,
        UserLogoutRequest request,
        CancellationToken ct)
    {
        var refreshToken = await _context.UserRefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == request.RefreshToken
                && rt.User.ApplicationId == applicationId, ct);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "User {UserId} logged out from application {AppId}",
                refreshToken.UserId,
                applicationId);
        }
        else
        {
            _logger.LogInformation(
                "Logout called with invalid or already revoked token for application {AppId}",
                applicationId);
        }

        return new UserLogoutResponse("Logged out successfully");
    }
}

public static class UserLogout
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/apps/{{appId:guid}}/auth/logout", async (
                Guid appId,
                UserLogoutRequest request,
                UserLogoutHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(appId, request, ct);
                return Results.Ok(ApiResponse<UserLogoutResponse>.Ok(response));
            })
            .WithName("UserLogout")
            .WithTags("User Auth")
            .AllowAnonymous(); 
    }
}
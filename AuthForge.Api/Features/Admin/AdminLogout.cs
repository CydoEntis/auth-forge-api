using AuthForge.Api.Common;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminLogoutRequest(string RefreshToken);

public sealed record AdminLogoutResponse(string Message);

public class AdminLogoutHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminLogoutHandler> _logger;

    public AdminLogoutHandler(AppDbContext context, ILogger<AdminLogoutHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminLogoutResponse> HandleAsync(
        AdminLogoutRequest request,
        CancellationToken ct)
    {
        var refreshToken = await _context.AdminRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Admin logged out, token revoked");
        }

        return new AdminLogoutResponse("Logged out successfully");
    }
}

public static class AdminLogout
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/admin/logout", async (
                AdminLogoutRequest request,
                AdminLogoutHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<AdminLogoutResponse>.Ok(response));
            })
            .WithName("AdminLogout")
            .WithTags("Admin")
            .RequireAuthorization();
    }
}
using AuthForge.Api.Common;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Auth;

public sealed record LogoutRequest(string RefreshToken);

public sealed record LogoutResponse(string Message);

public class LogoutHandler
{
    private readonly AppDbContext _context;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(AppDbContext context, ILogger<LogoutHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LogoutResponse> HandleAsync(
        LogoutRequest request,
        CancellationToken ct)
    {
        var refreshToken = await _context.AdminRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Account logged out, token revoked");
        }

        return new LogoutResponse("Logged out successfully");
    }
}

public static class Logout
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPost($"{prefix}/logout", async (
                LogoutRequest request,
                LogoutHandler handler,
                CancellationToken ct) =>
            {
                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<LogoutResponse>.Ok(response));
            })
            .WithName("Logout")
            .WithTags("Auth")
            .RequireAuthorization();
    }
}
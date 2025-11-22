using AuthForge.Api.Common;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Middleware;

// Validates that requests to user auth endpoints come from allowed origins.
// Each application can specify its own allowed origins.
public class CorsValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorsValidationMiddleware> _logger;

    public CorsValidationMiddleware(
        RequestDelegate next,
        ILogger<CorsValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (!IsUserAuthEndpoint(path))
        {
            await _next(context);
            return;
        }

        var appId = ExtractApplicationIdFromPath(context);

        if (appId == null)
        {
            await _next(context);
            return;
        }

        var origin = context.Request.Headers["Origin"].FirstOrDefault();

        if (string.IsNullOrEmpty(origin))
        {
            _logger.LogDebug(
                "Request to {Path} from app {AppId} has no Origin header",
                path,
                appId);

            await _next(context);
            return;
        }

        var application = await dbContext.Applications
            .Where(a => a.Id == appId.Value)
            .Select(a => new { a.AllowedOrigins, a.IsActive, a.IsDeleted })
            .FirstOrDefaultAsync();

        if (application == null || application.IsDeleted || !application.IsActive)
        {
            await _next(context);
            return;
        }

        if (application.AllowedOrigins == null || !application.AllowedOrigins.Any())
        {
            _logger.LogDebug(
                "Application {AppId} has no CORS restrictions - allowing request from {Origin}",
                appId,
                origin);

            await _next(context);
            return;
        }

        var normalizedOrigin = NormalizeOrigin(origin);
        var isAllowed = application.AllowedOrigins.Any(allowedOrigin =>
        {
            var normalizedAllowed = NormalizeOrigin(allowedOrigin);
            return string.Equals(normalizedAllowed, normalizedOrigin, StringComparison.OrdinalIgnoreCase);
        });

        if (!isAllowed)
        {
            _logger.LogWarning(
                "CORS violation: Request to {Path} from unauthorized origin {Origin} for app {AppId}. " +
                "Allowed origins: {AllowedOrigins}",
                path,
                origin,
                appId,
                string.Join(", ", application.AllowedOrigins));

            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                ErrorCodes.Forbidden,
                "Requests from this origin are not allowed for this application.",
                null));

            return;
        }

        context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");

        if (context.Request.Method == "OPTIONS")
        {
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            context.Response.Headers.Add("Access-Control-Max-Age", "86400");
            context.Response.StatusCode = 204;
            return;
        }

        _logger.LogDebug(
            "CORS validation passed for {Path} from origin {Origin} for app {AppId}",
            path,
            origin,
            appId);

        await _next(context);
    }

    private static bool IsUserAuthEndpoint(string path)
    {
        // CORS validation to user auth endpoints only
        return path.Contains("/apps/") &&
               (path.Contains("/auth/register") ||
                path.Contains("/auth/login") ||
                path.Contains("/auth/refresh") ||
                path.Contains("/auth/logout") ||
                path.Contains("/auth/forgot-password") ||
                path.Contains("/auth/reset-password") ||
                path.Contains("/auth/verify-email"));
    }

    private static Guid? ExtractApplicationIdFromPath(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("appId", out var appIdObj))
        {
            if (appIdObj is Guid guid)
                return guid;

            if (appIdObj is string appIdStr && Guid.TryParse(appIdStr, out var parsedGuid))
                return parsedGuid;
        }

        return null;
    }

    private static string NormalizeOrigin(string origin)
    {
        origin = origin.TrimEnd('/');

        if (!origin.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !origin.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            origin = "https://" + origin;
        }

        return origin;
    }
}
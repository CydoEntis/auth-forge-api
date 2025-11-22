using AuthForge.Api.Common;
using Microsoft.Extensions.Caching.Memory;

namespace AuthForge.Api.Middleware;


public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    private const int MaxRegistrationsPerHour = 5;
    private const int MaxLoginAttemptsPerHour = 20;
    private const int MaxPasswordResetRequestsPerHour = 5;
    private const int MaxRefreshTokenAttemptsPerHour = 50;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache cache)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        var (endpointType, maxRequests) = GetRateLimitForPath(path);

        if (endpointType == null)
        {
            await _next(context);
            return;
        }

        var ipAddress = GetClientIpAddress(context);

        if (string.IsNullOrEmpty(ipAddress))
        {
            _logger.LogWarning("Could not determine client IP address for rate limiting");
            await _next(context);
            return;
        }

        var cacheKey = $"rate_limit:{endpointType}:{ipAddress}";

        if (!cache.TryGetValue(cacheKey, out RateLimitCounter counter))
        {
            counter = new RateLimitCounter
            {
                Count = 0,
                WindowStart = DateTime.UtcNow
            };
        }

        if (DateTime.UtcNow - counter.WindowStart > TimeSpan.FromHours(1))
        {
            counter.Count = 0;
            counter.WindowStart = DateTime.UtcNow;
        }

        if (counter.Count >= maxRequests)
        {
            var resetTime = counter.WindowStart.AddHours(1);
            var minutesUntilReset = (int)(resetTime - DateTime.UtcNow).TotalMinutes;

            _logger.LogWarning(
                "Rate limit exceeded for {EndpointType} from IP {IP}. " +
                "Count: {Count}/{Max}. Resets in {Minutes} minutes.",
                endpointType,
                ipAddress,
                counter.Count,
                maxRequests,
                minutesUntilReset);

            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Add("Retry-After",
                ((int)TimeSpan.FromMinutes(minutesUntilReset).TotalSeconds).ToString());

            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                ErrorCodes.RateLimitExceeded,
                $"Too many {endpointType} attempts. Please try again in {minutesUntilReset} minute(s).",
                null));

            return;
        }

        counter.Count++;

        cache.Set(cacheKey, counter, TimeSpan.FromHours(1));

        _logger.LogDebug(
            "Rate limit check passed for {EndpointType} from IP {IP}. " +
            "Count: {Count}/{Max}",
            endpointType,
            ipAddress,
            counter.Count,
            maxRequests);

        await _next(context);
    }

    private static (string? endpointType, int maxRequests) GetRateLimitForPath(string path)
    {
        if (path.Contains("/auth/register"))
            return ("registration", MaxRegistrationsPerHour);

        if (path.Contains("/auth/login"))
            return ("login", MaxLoginAttemptsPerHour);

        if (path.Contains("/auth/forgot-password") || path.Contains("/auth/reset-password"))
            return ("password_reset", MaxPasswordResetRequestsPerHour);

        if (path.Contains("/auth/refresh"))
            return ("token_refresh", MaxRefreshTokenAttemptsPerHour);

        return (null, 0);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp.Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class RateLimitCounter
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
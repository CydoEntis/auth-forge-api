using AuthForge.Api.Attributes;
using AuthForge.Api.Common.Responses;
using AuthForge.Domain.Errors;
using Microsoft.Extensions.Caching.Memory;

namespace AuthForge.Api.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(
        RequestDelegate next,
        IMemoryCache memoryCache,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _cache = memoryCache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var rateLimitAttribute = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();

        if (rateLimitAttribute is null)
        {
            await _next(context);
            return;
        }

        var identifier = GetIdentifier(context);
        var path = context.Request.Path.ToString();
        var cacheKey = $"ratelimit:{identifier}:{path}";

        _logger.LogDebug(
            "Rate limit check for {Identifier} on {Path} - Limit: {MaxRequests}/{WindowMinutes}min",
            identifier,
            path,
            rateLimitAttribute.Requests,
            rateLimitAttribute.WindowMinutes);

        var rateLimitInfo = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(rateLimitAttribute.WindowMinutes);
            return new RateLimitInfo
            {
                Count = 0,
                WindowStart = DateTime.UtcNow
            };
        });

        var windowDuration = TimeSpan.FromMinutes(rateLimitAttribute.WindowMinutes);
        if (DateTime.UtcNow - rateLimitInfo!.WindowStart > windowDuration)
        {
            rateLimitInfo.Count = 0;
            rateLimitInfo.WindowStart = DateTime.UtcNow;
        }

        rateLimitInfo.Count++;

        if (rateLimitInfo.Count > rateLimitAttribute.Requests)
        {
            var retryAfter = (int)(windowDuration - (DateTime.UtcNow - rateLimitInfo.WindowStart)).TotalSeconds;
            context.Response.Headers["Retry-After"] = retryAfter.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = rateLimitAttribute.Requests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(rateLimitInfo.WindowStart.Add(windowDuration)).ToUnixTimeSeconds().ToString();

            _logger.LogWarning(
                "Rate limit exceeded for {Identifier} on {Path} - {Count}/{MaxRequests} requests in {WindowMinutes}min window. Retry after {RetryAfter}s",
                identifier,
                path,
                rateLimitInfo.Count,
                rateLimitAttribute.Requests,
                rateLimitAttribute.WindowMinutes,
                retryAfter);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            
            var error = RateLimitErrors.Exceeded(rateLimitAttribute.Requests, rateLimitAttribute.WindowMinutes);
            var errorResponse = ApiResponse<object>.FailureResponse(error);

            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        _cache.Set(cacheKey, rateLimitInfo, TimeSpan.FromMinutes(rateLimitAttribute.WindowMinutes));

        context.Response.Headers["X-RateLimit-Limit"] = rateLimitAttribute.Requests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (rateLimitAttribute.Requests - rateLimitInfo.Count).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(rateLimitInfo.WindowStart.Add(windowDuration)).ToUnixTimeSeconds().ToString();

        _logger.LogDebug(
            "Rate limit passed for {Identifier} on {Path} - {Count}/{MaxRequests} requests",
            identifier,
            path,
            rateLimitInfo.Count,
            rateLimitAttribute.Requests);

        await _next(context);
    }

    private static string GetIdentifier(HttpContext context)
    {
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
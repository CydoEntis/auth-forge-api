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

    public RateLimitMiddleware(RequestDelegate next, IMemoryCache memoryCache, ILogger<RateLimitMiddleware> logger)
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

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.ToString();

        var cacheKey = $"ratelimit:{ipAddress}:{path}";

        var currentCount = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(rateLimitAttribute.WindowMinutes);
            return 0;
        });

        if (currentCount >= rateLimitAttribute.Requests)
        {
            _logger.LogWarning("Rate limit exceeded for {IpAddress} on {Path}", ipAddress, path);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var error = RateLimitErrors.Exceeded(rateLimitAttribute.Requests, rateLimitAttribute.WindowMinutes);

            var errorResponse = ApiResponse<object>.FailureResponse(error);

            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        _cache.Set(cacheKey, currentCount + 1, TimeSpan.FromMinutes(rateLimitAttribute.WindowMinutes));
        context.Response.Headers["X-RateLimit-Limit"] = rateLimitAttribute.Requests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (rateLimitAttribute.Requests - currentCount - 1).ToString();

        await _next(context);
    }
}
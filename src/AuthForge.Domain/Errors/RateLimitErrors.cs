namespace AuthForge.Domain.Errors;

public static class RateLimitErrors
{
    public static Error Exceeded(int limit, int windowMinutes) => new(
        "RateLimit.Exceeded",
        $"Rate limit exceeded. Maximum {limit} requests per {windowMinutes} minute(s). Please try again later.");
}
using AuthForge.Api.Common.Exceptions;
using AuthForge.Api.Common.Interfaces;

namespace AuthForge.Api.Middleware;

public class SetupCheckMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] AllowedPaths =
    {
        "/health",
        "/api/v1/health", 
        "/api/v1/setup/status", 
        "/api/v1/setup/test-database", 
        "/api/v1/setup/test-email", 
        "/api/v1/setup/complete", 
        "/openapi", 
        "/docs" 
    };

    public SetupCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfigurationService configService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Allow development-only paths
        if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
        }

        // Allow requests if setup is complete
        var isSetupComplete = await configService.IsSetupCompleteAsync();
        if (isSetupComplete)
        {
            await _next(context);
            return;
        }

        // Allow requests to setup endpoints
        if (AllowedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        throw new SetupRequiredException();
    }
}
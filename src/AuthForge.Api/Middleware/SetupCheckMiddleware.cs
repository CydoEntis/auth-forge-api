using AuthForge.Application.Common.Interfaces;

namespace AuthForge.Api.Middleware;

/// <summary>
/// Middleware that checks if setup is complete and routes requests accordingly.
/// If setup is not complete, only setup endpoints are accessible.
/// </summary>
public class SetupCheckMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] SetupPaths =
    [
        "/api/setup/status",
        "/api/setup/test-database",
        "/api/setup/test-email",
        "/api/setup/complete"
    ];

    public SetupCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var setupService = context.RequestServices.GetRequiredService<ISetupService>();
        var isSetupComplete = await setupService.IsSetupCompleteAsync();

        var requestPath = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // If setup is not complete
        if (!isSetupComplete)
        {
            // Allow access to setup endpoints
            if (IsSetupEndpoint(requestPath))
            {
                await _next(context);
                return;
            }

            // Block all other endpoints
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new
                {
                    code = "Setup.Required",
                    message = "The application must be configured before use. Please complete the setup wizard."
                },
                setupUrl = "/api/setup/status"
            });
            return;
        }

        // Setup is complete, allow all requests
        await _next(context);
    }

    private static bool IsSetupEndpoint(string path)
    {
        return SetupPaths.Any(setupPath =>
            path.Equals(setupPath, StringComparison.OrdinalIgnoreCase));
    }
}
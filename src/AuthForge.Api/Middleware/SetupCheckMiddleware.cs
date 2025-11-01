namespace AuthForge.Api.Middleware;

public class SetupCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public SetupCheckMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isSetupComplete = _configuration.GetValue<bool>("Setup:IsComplete");
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (!isSetupComplete && !path.StartsWith("/api/setup"))
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new
                {
                    code = "Setup.Required",
                    message = "Setup must be completed before using the application"
                }
            });
            return;
        }

        await _next(context);
    }
}
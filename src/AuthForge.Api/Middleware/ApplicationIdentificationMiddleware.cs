using AuthForge.Application.Common.Interfaces;

namespace AuthForge.Api.Middleware;

public class ApplicationIdentificationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-AuthForge-Key";

    public ApplicationIdentificationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IApplicationRepository applicationRepository)
    {
        Console.WriteLine($"=== MIDDLEWARE DEBUG ===");
        Console.WriteLine($"Path: {context.Request.Path}");

        if (context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey))
        {
            Console.WriteLine($"API Key found: {apiKey}");

            var application = await applicationRepository.GetByPublicKeyAsync(
                apiKey.ToString(),
                context.RequestAborted);

            Console.WriteLine($"Application found: {application?.Name ?? "NULL"}");
            Console.WriteLine($"Application active: {application?.IsActive}");
            Console.WriteLine($"Allowed origins count: {application?.AllowedOrigins.Count ?? 0}");

            if (application is not null && application.IsActive)
            {
                var origin = context.Request.Headers["Origin"].ToString();
                Console.WriteLine($"Request origin: '{origin}'");
                Console.WriteLine($"Allowed origins: {string.Join(", ", application.AllowedOrigins)}");

                if (application.AllowedOrigins.Any())
                {
                    Console.WriteLine("Checking origin...");
                    if (string.IsNullOrEmpty(origin) || !application.AllowedOrigins.Contains(origin))
                    {
                        Console.WriteLine("BLOCKING REQUEST - Origin not allowed!");
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            error = new
                            {
                                code = "Auth.OriginNotAllowed",
                                message = string.IsNullOrEmpty(origin)
                                    ? "Origin header is required"
                                    : $"Origin '{origin}' is not allowed for this application"
                            }
                        });
                        return;
                    }

                    Console.WriteLine("Origin check passed!");
                }

                context.Items["Application"] = application;
                context.Items["ApplicationId"] = application.Id.Value.ToString();
            }
        }
        else
        {
            Console.WriteLine("No API key in request");
        }

        Console.WriteLine("Continuing to next middleware...");
        await _next(context);
    }
}
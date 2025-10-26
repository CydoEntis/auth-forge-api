using AuthForge.Application.Common.Interfaces;

namespace AuthForge.Api.Middleware;

public class ApplicationIdentificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApplicationIdentificationMiddleware> _logger;
    private const string ApiKeyHeader = "X-AuthForge-Key";

    public ApplicationIdentificationMiddleware(
        RequestDelegate next,
        ILogger<ApplicationIdentificationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IApplicationRepository applicationRepository)
    {
        _logger.LogDebug("Processing request for path {Path}", context.Request.Path);

        if (context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey))
        {
            _logger.LogDebug("API Key found in request header");

            var application = await applicationRepository.GetByPublicKeyAsync(
                apiKey.ToString(),
                context.RequestAborted);

            if (application is null)
            {
                _logger.LogWarning("Invalid API key provided: {ApiKey}", apiKey.ToString());
            }
            else if (!application.IsActive)
            {
                _logger.LogWarning(
                    "Inactive application attempted access: {ApplicationName} (ID: {ApplicationId})",
                    application.Name,
                    application.Id);
            }
            else
            {
                _logger.LogDebug(
                    "Application identified: {ApplicationName} (ID: {ApplicationId})",
                    application.Name,
                    application.Id);

                var origin = context.Request.Headers["Origin"].ToString();

                if (application.AllowedOrigins.Any())
                {
                    if (string.IsNullOrEmpty(origin) || !application.AllowedOrigins.Contains(origin))
                    {
                        _logger.LogWarning(
                            "Request blocked - Origin '{Origin}' not allowed for application {ApplicationName}. Allowed origins: {AllowedOrigins}",
                            origin,
                            application.Name,
                            string.Join(", ", application.AllowedOrigins));

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

                    _logger.LogDebug("Origin validation passed for {Origin}", origin);
                }

                context.Items["Application"] = application;
                context.Items["ApplicationId"] = application.Id.Value.ToString();
            }
        }
        else
        {
            _logger.LogDebug("No API key header present in request");
        }

        await _next(context);
    }
}
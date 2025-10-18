// src/AuthForge.Api/Middleware/ApplicationIdentificationMiddleware.cs

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
        if (context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKey))
        {
            var application = await applicationRepository.GetByPublicKeyAsync(
                apiKey.ToString(),
                context.RequestAborted);

            if (application is not null && application.IsActive)
            {
                context.Items["Application"] = application;
                context.Items["ApplicationId"] = application.Id.Value.ToString();
            }
        }

        await _next(context);
    }
}
using AuthForge.Api.Features.Admin;
using AuthForge.Api.Features.Setup;

namespace AuthForge.Api.Common.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapHealthChecks(this WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/health", () => Results.Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = app.Environment.EnvironmentName
            }))
            .WithName("HealthCheck")
            .WithTags("System")
            .AllowAnonymous();

        return app;
    }

    public static WebApplication MapFeatureEndpoints(this WebApplication app, string prefix = "/api/v1")
    {
        app.MapSetupEndpoints(prefix);
        app.MapAdminEndpoints(prefix);

        return app;
    }
}
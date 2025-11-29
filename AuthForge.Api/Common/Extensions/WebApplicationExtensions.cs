using AuthForge.Api.Features.Account;
using AuthForge.Api.Features.Applications;
using AuthForge.Api.Features.Auth;
using AuthForge.Api.Features.Email;
using AuthForge.Api.Features.Settings;
using AuthForge.Api.Features.Setup;
using AuthForge.Api.Features.Users;

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

    public static WebApplication MapFeatureEndpoints(this WebApplication app)
    {
        app.MapEmailEndpoints();
        app.MapSetupEndpoints();
        app.MapAuthEndpoints();
        app.MapAccountEndpoints();
        app.MapSettingsEndpoints();
        app.MapApplicationEndpoints();
        app.MapUserEndpoints();

        return app;
    }
}
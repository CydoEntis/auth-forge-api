namespace AuthForge.Api.Features.Applications;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CreateApplicationHandler>();

        return services;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app, string prefix = "/api/v1")
    {
        CreateApplication.MapEndpoints(app, prefix);
        GetApplication.MapEndpoints(app, prefix);
        GetApplicationSettings.MapEndpoints(app, prefix);
        HardDeleteApplication.MapEndpoints(app, prefix);
        ListApplications.MapEndpoints(app, prefix);
        ListDeletedApplications.MapEndpoints(app, prefix);
        SoftDeleteApplication.MapEndpoints(app, prefix);
        UpdateApplication.MapEndpoints(app, prefix);
        UpdateApplicationEmailProvider.MapEndpoints(app, prefix);
        UpdateApplicationOAuth.MapEndpoints(app, prefix);
        UpdateApplicationSecurity.MapEndpoints(app, prefix);

        return app;
    }
}
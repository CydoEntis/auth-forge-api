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

        return app;
    }
}
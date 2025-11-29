namespace AuthForge.Api.Features.Settings;

public static class SettingsModule
{
    public static IServiceCollection AddSettingsServices(this IServiceCollection services)
    {
        services.AddScoped<UpdateDomainHandler>();
        services.AddScoped<UpdateEmailProviderHandler>();
        services.AddScoped<GetSettingsHandler>();
        services.AddScoped<RegenerateJwtSecretHandler>();

        return services;
    }

    public static WebApplication MapSettingsEndpoints(this WebApplication app, string prefix = "/api/v1/settings")
    {
        UpdateDomain.MapEndpoints(app, prefix);
        UpdateEmailProvider.MapEndpoints(app, prefix);
        GetSettings.MapEndpoints(app, prefix);
        RegenerateJwtSecret.MapEndpoints(app, prefix);

        return app;
    }
}
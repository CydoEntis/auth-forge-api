namespace AuthForge.Api.Features.Setup;

public static class SetupModule
{
    public static IServiceCollection AddSetupServices(this IServiceCollection services)
    {
        services.AddScoped<CompleteSetupHandler>();
        services.AddScoped<TestDatabaseConnectionHandler>();
        services.AddScoped<GetSetupStatusHandler>();

        return services;
    }

    public static WebApplication MapSetupEndpoints(this WebApplication app, string prefix = "/api/v1")
    {
        CompleteSetupFeature.MapEndpoints(app, prefix);
        TestDatabaseConnectionFeature.MapEndpoints(app, prefix);
        GetSetupStatusFeature.MapEndpoints(app, prefix);

        return app;
    }
}
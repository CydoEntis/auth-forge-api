namespace AuthForge.Api.Features.Email;

public static class EmailModule
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services)
    {
        services.AddScoped<TestEmailProviderHandler>();


        return services;
    }

    public static WebApplication MapEmailEndpoints(this WebApplication app, string prefix = "/api/v1/email")
    {
        TestEmailProvider.MapEndpoints(app, prefix);
        return app;
    }
}
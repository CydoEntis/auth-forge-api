namespace AuthForge.Api.Endpoints.Applications;

public static class ApplicationsEndpointsRegistration
{
    public static IEndpointRouteBuilder MapApplicationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateApplicationEndpoint();
        app.MapGetApplicationByIdEndpoint();
        app.MapUpdateApplicationEndpoint();
        app.MapDeleteApplicationEndpoint();
        app.MapGetApplicationsEndpoint();
        app.MapAddAllowedOriginEndpoint();
        app.MapGetApplicationKeysEndpoint();
        app.MapRegenerateApplicationKeysEndpoint();
        app.MapConfigureEmailEndpoint();
        app.MapUpdateEmailEndpoint();

        return app;
    }
}
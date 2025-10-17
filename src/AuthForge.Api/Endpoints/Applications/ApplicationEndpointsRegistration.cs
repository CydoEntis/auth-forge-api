namespace AuthForge.Api.Endpoints.Applications;

public static class ApplicationsEndpointsRegistration
{
    public static IEndpointRouteBuilder MapApplicationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateApplicationEndpoint();
        app.MapGetApplicationByIdEndpoint();
        app.MapUpdateApplicationEndpoint();
        app.MapDeleteApplicationEndpoint();

        return app;
    }
}
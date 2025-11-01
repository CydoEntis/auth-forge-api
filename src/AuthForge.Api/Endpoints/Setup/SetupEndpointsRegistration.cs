namespace AuthForge.Api.Endpoints.Setup;

public static class SetupEndpointsRegistration
{
    public static IEndpointRouteBuilder MapSetupEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetSetupStatusEndpoint();
        app.MapTestDatabaseConnectionEndpoint();
        app.MapTestEmailEndpoint();
        app.MapCompleteSetupEndpoint();

        return app;
    }
}
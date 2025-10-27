namespace AuthForge.Api.Endpoints.Admin;

public static class AdminEndpointsRegistration
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapSetupAdminEndpoint(); 
        app.MapLoginAdminEndpoint();
        app.MapRefreshAdminTokenEndpoint();
        app.MapRequestAdminPasswordResetEndpoint();
        app.MapResetAdminPasswordEndpoint();
        app.MapGetCurrentAdminEndpoint();
        app.MapChangeAdminPasswordEndpoint();
        app.MapGetSetupStatusEndpoint();
        
        return app;
    }
}
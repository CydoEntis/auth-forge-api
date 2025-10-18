namespace AuthForge.Api.Endpoints.Admin;

public static class AdminEndpointsRegistration
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapLoginAdminEndpoint();
        app.MapRefreshAdminTokenEndpoint();
        
        return app;
    }
}
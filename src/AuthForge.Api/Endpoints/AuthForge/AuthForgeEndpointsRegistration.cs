namespace AuthForge.Api.Endpoints.AuthForge;

public static class AuthForgeEndpointsRegistration
{
    public static IEndpointRouteBuilder MapAuthForgeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterDeveloperEndpoint();
        app.MapLoginDeveloperEndpoint();
        app.MapRefreshDeveloperTokenEndpoint();
        return app;
    }
}

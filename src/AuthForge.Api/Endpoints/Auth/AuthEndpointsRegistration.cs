namespace AuthForge.Api.Endpoints.Auth;

public static class AuthEndpointsRegistration
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterEndpoint();
        app.MapLoginEndpoint();
        app.MapRefreshEndpoint();
        return app;
    }
}
namespace AuthForge.Api.Endpoints.EndUsers;

public static class EndUserEndpointsRegistration
{
    public static IEndpointRouteBuilder MapEndUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterEndUserEndpoint();
        app.MapLoginEndUserEndpoint();
        app.MapRefreshEndUserTokenEndpoint();
        return app;
    }
}

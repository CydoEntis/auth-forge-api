using AuthForge.Api.Endpoints.Auth;

namespace AuthForge.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterEndpoint();

        return app;
    }
}
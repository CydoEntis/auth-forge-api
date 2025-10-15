using AuthForge.Api.Endpoints.Applications;
using AuthForge.Api.Endpoints.AuthForge;
using AuthForge.Api.Endpoints.EndUsers;

namespace AuthForge.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAuthForgeEndpoints();
        app.MapEndUserEndpoints();
        app.MapApplicationsEndpoints();
        
        return app;
    }
}
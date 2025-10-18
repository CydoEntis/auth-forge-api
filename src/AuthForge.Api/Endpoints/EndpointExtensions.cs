using AuthForge.Api.Endpoints.Admin;
using AuthForge.Api.Endpoints.Applications;
using AuthForge.Api.Endpoints.EndUsers;

namespace AuthForge.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAdminEndpoints();
        app.MapEndUserEndpoints();
        app.MapApplicationsEndpoints();

        return app;
    }
}
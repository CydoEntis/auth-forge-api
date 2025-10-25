namespace AuthForge.Api.Endpoints.AuditLogs;

public static class AuditLogEndpointsRegistration
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetAuditLogsEndpoint();

        return app;
    }
}
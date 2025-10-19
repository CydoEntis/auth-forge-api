namespace AuthForge.Api.Endpoints.EndUsers;

public static class EndUserEndpointsRegistration
{
    public static IEndpointRouteBuilder MapEndUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterEndUserEndpoint();
        app.MapLoginEndUserEndpoint();
        app.MapRefreshEndUserTokenEndpoint();
        app.MapGetEndUsersEndpoint();
        app.MapForgotPasswordEndpoint();
        app.MapResetPasswordEndpoint(); 
        app.MapChangePasswordEndpoint();
        app.MapSendVerificationEmailEndpoint();
        app.MapVerifyEmailEndpoint();
        
        return app;
    }
}

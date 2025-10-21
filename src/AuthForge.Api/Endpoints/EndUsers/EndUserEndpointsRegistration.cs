namespace AuthForge.Api.Endpoints.EndUsers;

public static class EndUserEndpointsRegistration
{
    public static IEndpointRouteBuilder MapEndUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegisterEndUserEndpoint();
        app.MapLoginEndUserEndpoint();
        app.MapRefreshEndUserTokenEndpoint();
        app.MapGetEndUsersEndpoint();
        app.MapGetEndUserByIdEndpoint();
        app.MapForgotPasswordEndpoint();
        app.MapResetPasswordEndpoint();
        app.MapChangePasswordEndpoint();
        app.MapSendVerificationEmailEndpoint();
        app.MapVerifyEmailEndpoint();
        app.MapGetCurrentUserEndpoint();
        app.MapUpdateCurrentUserEndpoint();
        app.MapDeactivateEndUserEndpoint();
        app.MapActivateEndUserEndpoint();
        app.MapUnlockEndUserEndpoint();
        app.MapLockEndUserEndpoint();
        app.MapManualVerifyEmailEndpoint();
        app.MapDeleteEndUserEndpoint();
        
        return app;
    }
}

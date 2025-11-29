namespace AuthForge.Api.Features.Account;

public static class AccountModule
{
    public static IServiceCollection AddAccountServices(this IServiceCollection services)
    {
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<UpdateEmailHandler>();
        services.AddScoped<RevokeAllSessionsHandler>();
        services.AddScoped<GetAccountHandler>();

        return services;
    }

    public static WebApplication MapAccountEndpoints(this WebApplication app, string prefix = "/api/v1/account")
    {
        ChangePassword.MapEndpoints(app, prefix);
        UpdateEmail.MapEndpoints(app, prefix);
        RevokeAllSessions.MapEndpoints(app, prefix);
        GetAccount.MapEndpoints(app, prefix);

        return app;
    }
}
namespace AuthForge.Api.Features.Applications;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CreateApplicationHandler>();
        services.AddScoped<GetApplicationHandler>();
        services.AddScoped<GetApplicationSettingsHandler>();
        services.AddScoped<HardDeleteApplicationHandler>();
        services.AddScoped<ListApplicationsHandler>();
        services.AddScoped<ListDeletedApplicationsHandler>();
        services.AddScoped<SoftDeleteApplicationHandler>();
        services.AddScoped<UpdateApplicationHandler>();
        services.AddScoped<UpdateApplicationEmailProviderHandler>();
        services.AddScoped<UpdateApplicationOAuthHandler>();
        services.AddScoped<UpdateApplicationSecurityHandler>();
        services.AddScoped<RegenerateApplicationClientSecretHandler>();
        services.AddScoped<RegenerateApplicationJwtSecretHandler>();
        services.AddScoped<RestoreApplicationHandler>();
        services.AddScoped<ListUsersHandler>();
        services.AddScoped<GetUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<LockUserHandler>();
        services.AddScoped<UnlockUserHandler>();
        services.AddScoped<RevokeUserSessionsHandler>();

        return services;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app, string prefix = "/api/v1")
    {
        CreateApplication.MapEndpoints(app, prefix);
        GetApplication.MapEndpoints(app, prefix);
        GetApplicationSettings.MapEndpoints(app, prefix);
        HardDeleteApplication.MapEndpoints(app, prefix);
        ListApplications.MapEndpoints(app, prefix);
        ListDeletedApplications.MapEndpoints(app, prefix);
        SoftDeleteApplication.MapEndpoints(app, prefix);
        UpdateApplication.MapEndpoints(app, prefix);
        UpdateApplicationEmailProvider.MapEndpoints(app, prefix);
        UpdateApplicationOAuth.MapEndpoints(app, prefix);
        UpdateApplicationSecurity.MapEndpoints(app, prefix);
        RegenerateApplicationClientSecret.MapEndpoints(app, prefix);
        RegenerateJwtSecretFeature.MapEndpoints(app, prefix);
        RestoreApplicationFeature.MapEndpoints(app, prefix);
        ListUsers.MapEndpoints(app, prefix);
        GetUser.MapEndpoints(app, prefix);
        DeleteUser.MapEndpoints(app, prefix);
        LockUser.MapEndpoints(app, prefix);
        UnlockUser.MapEndpoints(app, prefix);
        RevokeUserSessions.MapEndpoints(app, prefix);
        
        return app;
    }
}
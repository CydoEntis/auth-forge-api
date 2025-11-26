namespace AuthForge.Api.Features.Admin;

public static class AdminModule
{
    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<AdminLoginHandler>();
        services.AddScoped<AdminRefreshTokenHandler>();
        services.AddScoped<AdminLogoutHandler>();

        // Password Management
        services.AddScoped<AdminForgotPasswordHandler>();
        services.AddScoped<AdminVerifyPasswordResetTokenHandler>();
        services.AddScoped<AdminResetPasswordHandler>();
        services.AddScoped<AdminChangePasswordHandler>();

        // Settings
        services.AddScoped<AdminUpdateEmailHandler>();
        services.AddScoped<AdminUpdateDomainHandler>();
        services.AddScoped<AdminUpdateEmailProviderHandler>();
        services.AddScoped<AdminGetSettingsHandler>();

        // Security
        services.AddScoped<AdminRevokeAllSessionsHandler>();
        services.AddScoped<AdminRegenerateJwtSecretHandler>();

        // Profile
        services.AddScoped<GetAdminHandler>();

        return services;
    }

    public static WebApplication MapAdminEndpoints(this WebApplication app, string prefix = "/api/v1")
    {
        // Auth
        AdminLogin.MapEndpoints(app, prefix);
        AdminRefreshToken.MapEndpoints(app, prefix);
        AdminLogout.MapEndpoints(app, prefix);

        // Password Management
        AdminForgotPassword.MapEndpoints(app, prefix);
        AdminVerifyPasswordResetToken.MapEndpoints(app, prefix);
        AdminResetPassword.MapEndpoints(app, prefix);
        AdminChangePassword.MapEndpoints(app, prefix);

        // Settings
        AdminUpdateEmail.MapEndpoints(app, prefix);
        AdminUpdateDomain.MapEndpoints(app, prefix);
        AdminUpdateEmailProvider.MapEndpoints(app, prefix);
        AdminGetSettings.MapEndpoints(app, prefix);

        // Security
        AdminRevokeAllSessions.MapEndpoints(app, prefix);
        AdminRegenerateJwtSecret.MapEndpoints(app, prefix);

        // Profile
        GetAdmin.MapEndpoints(app, prefix);

        return app;
    }
}
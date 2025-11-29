namespace AuthForge.Api.Features.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<VerifyPasswordResetTokenHandler>();
        services.AddScoped<ResetPasswordHandler>();

        return services;
    }

    public static WebApplication MapAuthEndpoints(this WebApplication app, string prefix = "/api/v1/auth")
    {
        Login.MapEndpoints(app, prefix);
        RefreshToken.MapEndpoints(app, prefix);
        Logout.MapEndpoints(app, prefix);
        ForgotPassword.MapEndpoints(app, prefix);
        VerifyPasswordResetToken.MapEndpoints(app, prefix);
        ResetPassword.MapEndpoints(app, prefix);

        return app;
    }
}
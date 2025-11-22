namespace AuthForge.Api.Features.Users;

public static class UserModule
{
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        // Authentication
        services.AddScoped<UserRegisterHandler>();
        services.AddScoped<UserLoginHandler>();
        services.AddScoped<UserRefreshTokenHandler>();
        services.AddScoped<UserLogoutHandler>();

        // TODO: Password Management 
        // services.AddScoped<UserForgotPasswordHandler>();
        // services.AddScoped<UserResetPasswordHandler>();
        // services.AddScoped<UserVerifyEmailHandler>();
        // services.AddScoped<UserResendVerificationHandler>();

        return services;
    }

    public static WebApplication MapUserEndpoints(this WebApplication app, string prefix = "/api/v1")
    {
        // Authentication
        UserRegister.MapEndpoints(app, prefix);
        UserLogin.MapEndpoints(app, prefix);
        UserRefreshToken.MapEndpoints(app, prefix);
        UserLogout.MapEndpoints(app, prefix);

        // TODO: Password Management 
        // UserForgotPassword.MapEndpoints(app, prefix);
        // UserResetPassword.MapEndpoints(app, prefix);
        // UserVerifyEmail.MapEndpoints(app, prefix);
        // UserResendVerification.MapEndpoints(app, prefix);

        return app;
    }
}
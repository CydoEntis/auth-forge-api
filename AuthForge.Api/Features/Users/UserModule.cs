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

        // Password Management
        services.AddScoped<UserForgotPasswordHandler>();
        services.AddScoped<UserResetPasswordHandler>();
        services.AddScoped<UserVerifyPasswordResetTokenHandler>();
        services.AddScoped<UserChangePasswordHandler>();

        // Email Verification
        services.AddScoped<UserVerifyEmailHandler>();
        services.AddScoped<UserResendVerificationEmailHandler>();

        // Profile
        services.AddScoped<GetCurrentUserHandler>();

        // Token Introspection
        services.AddScoped<IntrospectTokenHandler>();

        // OAuth
        services.AddScoped<GoogleAuthorizeHandler>();
        services.AddScoped<GoogleCallbackHandler>();
        services.AddScoped<GithubAuthorizeHandler>();
        services.AddScoped<GithubCallbackHandler>();

        return services;
    }

    public static WebApplication MapUserEndpoints(this WebApplication app, string prefix = "/api/v1")
    {
        // Authentication
        UserRegister.MapEndpoints(app, prefix);
        UserLogin.MapEndpoints(app, prefix);
        UserRefreshToken.MapEndpoints(app, prefix);
        UserLogout.MapEndpoints(app, prefix);

        // Password Management
        UserForgotPassword.MapEndpoints(app, prefix);
        UserResetPassword.MapEndpoints(app, prefix);
        UserVerifyPasswordResetToken.MapEndpoints(app, prefix);
        UserChangePassword.MapEndpoints(app, prefix);

        // Email Verification
        UserVerifyEmail.MapEndpoints(app, prefix);
        UserResendVerificationEmail.MapEndpoints(app, prefix);

        // Profile
        GetCurrentUser.MapEndpoints(app, prefix);

        // Token Introspection
        UserTokenIntrospection.MapEndpoints(app, prefix);

        // OAuth
        GoogleAuthorize.MapEndpoints(app, prefix);
        GoogleCallback.MapEndpoints(app, prefix);
        GithubAuthorize.MapEndpoints(app, prefix);
        GithubCallback.MapEndpoints(app, prefix);
        
        return app;
    }
}
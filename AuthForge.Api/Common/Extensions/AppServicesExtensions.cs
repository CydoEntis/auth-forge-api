using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Common.Services;
using AuthForge.Api.Entities;
using AuthForge.Api.Features.Account;
using AuthForge.Api.Features.Applications;
using AuthForge.Api.Features.Auth;
using AuthForge.Api.Features.Email;
using AuthForge.Api.Features.Settings;
using AuthForge.Api.Features.Setup;
using AuthForge.Api.Features.Users;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace AuthForge.Api.Common.Extensions;

public static class AppServicesExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        // Data Protection
        var dataProtectionPath = Path.Combine(environment.ContentRootPath, "Data", "Keys");
        Directory.CreateDirectory(dataProtectionPath);

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
            .SetApplicationName("AuthForge");

        services.AddHttpContextAccessor();

        // Services
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailServiceFactory, EmailServiceFactory>();
        services.AddScoped<IEmailTestService, EmailTestService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddHttpClient();
        services.AddScoped<IOAuthService, OAuthService>();

        // Password Hashers
        services.AddSingleton<PasswordHasher<Admin>>();
        services.AddSingleton<PasswordHasher<User>>();

        // Fluent Validation
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Modules
        services.AddEmailServices();
        services.AddSetupServices();
        services.AddAuthServices();
        services.AddAccountServices();
        services.AddSettingsServices();
        services.AddApplicationServices();
        services.AddUserServices();
        return services;
    }
}
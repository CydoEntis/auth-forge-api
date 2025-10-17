using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Authentication;
using AuthForge.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace AuthForge.Infrastructure.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IEndUserJwtTokenGenerator, EndUserJwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        
        return services;
    }
}
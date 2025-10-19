using System.Reflection;
using AuthForge.Application.Common.Behaviors;
using AuthForge.Application.Common.Services;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AuthForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IEmailParser, EmailParser>();

        return services;
    }
}
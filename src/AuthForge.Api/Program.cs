using AuthForge.Api.Configuration;
using AuthForge.Api.Endpoints;
using AuthForge.Api.Middleware;
using AuthForge.Application;
using AuthForge.Infrastructure;
using Serilog;

SerilogConfiguration.ConfigureSerilog();

try
{
    Log.Information("Starting AuthForge API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                         ?? new[] { "http://localhost:3000" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    builder.Services.AddOpenApi();
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AuthForge.Infrastructure.Data.AuthForgeDbContext>(
            name: "database",
            tags: new[] { "db", "ready" })
        .AddCheck<AuthForge.Api.HealthChecks.EmailServiceHealthCheck>(
            name: "email_service",
            tags: new[] { "email", "ready" });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
        options.AddPolicy("EndUser", policy => policy.RequireAuthenticatedUser());
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }


    app.ConfigureSerilogRequestLogging();

    app.UseCors("AllowFrontend");

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    app.UseAuthentication();
    app.UseMiddleware<ApplicationIdentificationMiddleware>();
    app.UseAuthorization();

    app.UseMiddleware<RateLimitMiddleware>();

    app.MapEndpoints();


    app.MapHealthChecks("/api/health");
    app.MapHealthChecks("/api/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.ToString(),
                entries = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.ToString(),
                    description = e.Value.Description
                })
            });
            await context.Response.WriteAsync(result);
        }
    });


    Log.Information("AuthForge API started successfully on {Urls}", string.Join(", ", app.Urls));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AuthForge API terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
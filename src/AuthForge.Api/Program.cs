using AuthForge.Api.Configuration;
using AuthForge.Api.Endpoints;
using AuthForge.Api.Middleware;
using AuthForge.Application;
using AuthForge.Infrastructure;
using AuthForge.Infrastructure.Data;
using Serilog;

SerilogConfiguration.ConfigureSerilog();

try
{
    Log.Information("Starting AuthForge API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "data");
    Directory.CreateDirectory(dataDirectory);

    var configDbLogger = LoggerFactory.Create(config => config.AddConsole())
        .CreateLogger<ConfigurationDatabase>();
    var configDb = new ConfigurationDatabase(dataDirectory, configDbLogger);

    var setupComplete = await configDb.GetBoolAsync("setup_complete");
    Log.Information("Setup complete: {SetupComplete}", setupComplete);

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

    builder.Services.AddDataProtection();

    builder.Services.AddOpenApi();
    
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        
            options.JsonSerializerOptions.PropertyNamingPolicy = 
                System.Text.Json.JsonNamingPolicy.CamelCase;
        });
    
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHealthChecks();
    builder.Services.AddSingleton(configDb);
    builder.Services.AddApplication();


    Log.Information("Setup complete: {SetupComplete}", setupComplete);
    Log.Information("Registering infrastructure services with dynamic configuration");

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = builder.Environment.IsDevelopment();
        options.ValidateOnBuild = builder.Environment.IsDevelopment();
    });

    Log.Information("Building application");
    var app = builder.Build();

    var dataProtectionProvider = app.Services.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>();
    configDb.SetDataProtector(dataProtectionProvider);

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.ConfigureSerilogRequestLogging();
    app.UseCors("AllowFrontend");

    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    app.UseMiddleware<SetupCheckMiddleware>();

    app.UseAuthentication();
    app.UseMiddleware<ApplicationIdentificationMiddleware>();
    app.UseAuthorization();
    app.UseMiddleware<RateLimitMiddleware>();

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

    app.MapEndpoints();

    Log.Information("AuthForge API started successfully");
    app.Run();
}
catch (HostAbortedException)
{
    Log.Warning("Host was aborted");
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

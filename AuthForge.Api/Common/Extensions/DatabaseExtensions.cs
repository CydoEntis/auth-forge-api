using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Common.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabases(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        var configDbPath = Path.Combine(
            environment.ContentRootPath,
            "Data",
            "Databases",
            "config.db");

        Directory.CreateDirectory(Path.GetDirectoryName(configDbPath)!);

        services.AddDbContext<ConfigDbContext>(options => { options.UseSqlite($"Data Source={configDbPath}"); });

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configService = serviceProvider.GetRequiredService<IConfigurationService>();
            var encryptionService = serviceProvider.GetRequiredService<IEncryptionService>(); 

            var config = configService.GetAsync().GetAwaiter().GetResult();

            if (config == null || !config.IsSetupComplete)
            {
                options.UseInMemoryDatabase("SetupDb");
                return;
            }

            var dbType = config.DatabaseProvider?.ToLower();
            var encryptedConnectionString = config.DatabaseConnectionString;

            if (string.IsNullOrEmpty(encryptedConnectionString))
            {
                throw new InvalidOperationException("Database connection string is not configured");
            }

            var connectionString = encryptionService.Decrypt(encryptedConnectionString);

            switch (dbType)
            {
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                case "postgresql":
                case "postgres":
                    options.UseNpgsql(connectionString);
                    break;
                case "sqlserver":
                case "mssql":
                    options.UseSqlServer(connectionString);
                    break;
                case "mysql":
                case "mariadb":
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported database type: {dbType}");
            }

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    public static async Task<WebApplication> EnsureConfigDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var configDb = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
        await configDb.Database.MigrateAsync();
        app.Logger.LogInformation("Config database initialized");
        return app;
    }

    public static async Task<WebApplication> MigrateMainDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
        var isSetupComplete = await configService.IsSetupCompleteAsync();

        if (!isSetupComplete)
        {
            app.Logger.LogInformation("Skipping main database migration - setup not complete");
            return app;
        }

        var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await appDb.Database.MigrateAsync();
            app.Logger.LogInformation("Main database migration completed");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to migrate main database");
            throw;
        }

        return app;
    }
}
using AuthForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthForge.Infrastructure.Extensions;

public static class DatabaseServiceExtensions
{
    private const string DbLocation = "data";
    private const string DbName = "authforge.db";
    private const string Postgres = "postgres";
    private const string Postgresql = "postgresql";
    private const string Mysql = "mysql";
    private const string SqlServer = "sqlserver";
    private const string Mssql = "mssql";
    private const string Sqlite = "sqlite";

    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthForgeDbContext>((serviceProvider, options) =>
        {
            var configDb = serviceProvider.GetRequiredService<ConfigurationDatabase>();

            var setupComplete = configDb.GetBoolAsync("setup_complete").GetAwaiter().GetResult();

            if (!setupComplete)
            {
                // During setup mode, configure a placeholder database connection.
                // This is never actually used because SetupCheckMiddleware blocks all requests
                // that would need the database. We configure it to satisfy EF Core's requirement
                // for a valid database provider, but the connection won't be opened.
                options.UseSqlite(
                    "Data Source=:memory:",
                    sqliteOptions => sqliteOptions.MigrationsAssembly(
                        typeof(AuthForgeDbContext).Assembly.FullName));
                Console.WriteLine("Setup mode: DbContext configured with in-memory placeholder (not used)");
                return;
            }

            // Load configuration from config database
            var settings = configDb.GetAllAsync().GetAwaiter().GetResult();
            var dbType = settings.GetValueOrDefault("database_type", "Sqlite");

            string connectionString;
            if (dbType.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                var dbPath = Path.Combine(Directory.GetCurrentDirectory(), DbLocation, DbName);
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                connectionString = $"Data Source={dbPath}";
            }
            else
            {
                connectionString = settings.GetValueOrDefault("postgres_connection_string", "");
            }

            ConfigureDatabaseProvider(options, connectionString, dbType);
        });

        return services;
    }

    private static void ConfigureDatabaseProvider(
        DbContextOptionsBuilder options,
        string connectionString,
        string provider)
    {
        switch (provider.ToLower())
        {
            case Postgresql:
            case Postgres:
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                        typeof(AuthForgeDbContext).Assembly.FullName));
                Console.WriteLine("Using PostgreSQL database");
                break;

            case Mysql:
                var serverVersion = ServerVersion.AutoDetect(connectionString);
                options.UseMySql(
                    connectionString,
                    serverVersion,
                    mySqlOptions => mySqlOptions.MigrationsAssembly(
                        typeof(AuthForgeDbContext).Assembly.FullName));
                Console.WriteLine("Using MySQL database");
                break;

            case SqlServer:
            case Mssql:
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions => sqlServerOptions.MigrationsAssembly(
                        typeof(AuthForgeDbContext).Assembly.FullName));
                Console.WriteLine("Using SQL Server database");
                break;

            case Sqlite:
            default:
                options.UseSqlite(
                    connectionString,
                    sqliteOptions => sqliteOptions.MigrationsAssembly(
                        typeof(AuthForgeDbContext).Assembly.FullName));
                Console.WriteLine($"Using SQLite database: {connectionString}");
                break;
        }
    }
}
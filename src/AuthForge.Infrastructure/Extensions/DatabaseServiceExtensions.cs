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
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseProvider = configuration["DatabaseProvider"] ?? "SQLite";

        if (string.IsNullOrEmpty(connectionString))
        {
            // Default to SQLite if no connection string provided
            AddSqliteDatabase(services);
        }
        else
        {
            AddDatabaseProvider(services, connectionString, databaseProvider);
        }

        return services;
    }

    private static void AddSqliteDatabase(IServiceCollection services)
    {
        var dbPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            DbLocation,
            DbName);

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<AuthForgeDbContext>(options =>
            options.UseSqlite(
                $"Data Source={dbPath}",
                sqliteOptions => sqliteOptions.MigrationsAssembly(
                    typeof(AuthForgeDbContext).Assembly.FullName)));

        Console.WriteLine($"Using SQLite database at: {dbPath}");
    }

    private static void AddDatabaseProvider(
        IServiceCollection services,
        string connectionString,
        string provider)
    {
        services.AddDbContext<AuthForgeDbContext>(options =>
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
                    options.UseSqlite(
                        connectionString,
                        sqliteOptions => sqliteOptions.MigrationsAssembly(
                            typeof(AuthForgeDbContext).Assembly.FullName));
                    Console.WriteLine("Using SQLite database");
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported database provider: {provider}. " +
                        $"Supported providers: SQLite, PostgreSQL, MySQL, SqlServer");
            }
        });
    }
}
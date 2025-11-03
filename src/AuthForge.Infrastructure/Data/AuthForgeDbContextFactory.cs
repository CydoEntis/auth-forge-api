using AuthForge.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthForge.Infrastructure.Data;

public class AuthForgeDbContextFactory : IDesignTimeDbContextFactory<AuthForgeDbContext>
{
    public AuthForgeDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = config["DatabaseProvider"] ?? "sqlite";
        var connectionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AuthForgeDbContext>();

        switch (provider.ToLower())
        {
            case "postgres":
            case "postgresql":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case "mysql":
                var serverVersion = ServerVersion.AutoDetect(connectionString);
                optionsBuilder.UseMySql(connectionString, serverVersion);
                break;
            case "sqlserver":
            case "mssql":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case "sqlite":
            default:
                var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "authforge.db");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                break;
        }

        return new AuthForgeDbContext(optionsBuilder.Options, new NoOpEncryptionService());
    }
}
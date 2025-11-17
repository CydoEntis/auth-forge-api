using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthForge.Api.Data;

/// Design-time factory for creating ConfigDbContext during migrations.
/// This is ONLY used by EF Core tooling (dotnet ef migrations).
/// At runtime, the real registration in DatabaseExtensions is used.
public class ConfigDbContextFactory : IDesignTimeDbContextFactory<ConfigDbContext>
{
    public ConfigDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ConfigDbContext>();
        
        // This is ONLY for generating migrations, not runtime
        optionsBuilder.UseSqlite("Data Source=config.db");

        return new ConfigDbContext(optionsBuilder.Options);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthForge.Api.Data;

public class ConfigDbContextFactory : IDesignTimeDbContextFactory<ConfigDbContext>
{
    public ConfigDbContext CreateDbContext(string[] args)
    {
        var projectRoot = Directory.GetCurrentDirectory();
        
        var dbPath = Path.Combine(projectRoot, "Data", "Databases", "config.db");
        
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        Console.WriteLine($"[EF Design-Time] Using config database at: {dbPath}");
        
        var optionsBuilder = new DbContextOptionsBuilder<ConfigDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new ConfigDbContext(optionsBuilder.Options);
    }
}
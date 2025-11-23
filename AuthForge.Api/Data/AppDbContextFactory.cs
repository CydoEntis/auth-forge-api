using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthForge.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var projectRoot = Directory.GetCurrentDirectory();
        
        var dbPath = Path.Combine(projectRoot, "Data", "Databases", "authforge.db");
        
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        Console.WriteLine($"[EF Design-Time] Using database at: {dbPath}");
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new AppDbContext(optionsBuilder.Options);
    }
}
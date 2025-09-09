using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework;

public class BonusSystemContextFactory : IDesignTimeDbContextFactory<BonusSystemContext>
{
    public BonusSystemContext CreateDbContext(string[] args)
    {
        // Get the current assembly's directory (BonusSystem.Infrastructure)
        var libraryDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            
        // Navigate up to the solution directory and then to the API project
        // This works assuming a standard solution structure
        // Format: bin/Debug/net7.0/ -> up 3 levels -> to Api project
        var apiProjectDir = Path.GetFullPath(Path.Combine(libraryDir, "..", "..", "..", "..", "BonusSystem.Api"));
            
        if (!Directory.Exists(apiProjectDir))
        {
            throw new DirectoryNotFoundException(
                $"API project directory not found at {apiProjectDir}. " +
                $"Please adjust the path calculation in BonusSystemContextFactory.");
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(apiProjectDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true);

        var configuration = builder.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<BonusSystemContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        return new BonusSystemContext(optionsBuilder.Options);
    }
}
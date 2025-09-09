using Microsoft.Extensions.DependencyInjection;

namespace BonusSystem.Infrastructure.DataAccess.Seeding;

public static class SeederExtensions
{
    public static IServiceCollection AddDatabaseSeeding(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        return services;
    }

    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}
using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Core.Common.Implementations.BonusExecutor;

public class NoActiveBonusRemoveService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<NoActiveBonusRemoveService> _logger;

    public NoActiveBonusRemoveService(IServiceProvider provider, ILogger<NoActiveBonusRemoveService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _provider.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var affectedUsers = await userRepository.RemoveAllBonusesFromAllUsersAsync();
                _logger.LogInformation("Quarterly bonus reset completed at: {Time}. Users affected: {Count}", DateTime.UtcNow, affectedUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during quarterly bonus reset");
            }

            var delay = GetDelayUntilNextQuarter();
            await Task.Delay(delay, cancellationToken);
        }
    }


    private static TimeSpan GetDelayUntilNextQuarter()
    {
        var now = DateTime.UtcNow;
        var currentQuarter = (now.Month - 1) / 3 + 1;

        var nextQuarterMonth = currentQuarter * 3 + 1;
        var nextQuarterYear = now.Year;

        if (nextQuarterMonth > 12)
        {
            nextQuarterMonth = 1;
            nextQuarterYear++;
        }

        var nextQuarterStart = new DateTime(nextQuarterYear, nextQuarterMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        return nextQuarterStart - now;
    }
}

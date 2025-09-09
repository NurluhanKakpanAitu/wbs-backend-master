using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Core.Common.Implementations.BonusExecutor;

public class CommissionExecutor : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<CommissionExecutor> _logger;

    public CommissionExecutor(IServiceProvider provider, ILogger<CommissionExecutor> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow; 
                var nextRun = new DateTime(now.Year, now.Month, 1).AddMonths(1); 
                var delay = nextRun - now;

                _logger.LogInformation("CommissionExecutor sleeping for {Delay}", delay);

                await Task.Delay(delay, stoppingToken);

                using var scope = _provider.CreateScope();
                var bonusRepo = scope.ServiceProvider.GetRequiredService<ICompanyBffService>();

                var result = await bonusRepo.SendCommissionNotificationAsync();

                _logger.LogInformation(
                    "Notification Sended"
                );
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monthly commission execution");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

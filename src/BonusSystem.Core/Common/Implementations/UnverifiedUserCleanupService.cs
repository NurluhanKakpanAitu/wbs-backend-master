using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BonusSystem.Core.Repositories;

namespace BonusSystem.Core.Common.Implementations.UserCleanup
{
    public class UnverifiedUserCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UnverifiedUserCleanupService> _logger;

        public UnverifiedUserCleanupService(IServiceScopeFactory scopeFactory, ILogger<UnverifiedUserCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UnverifiedUserCleanupService запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var cutoffTime = DateTime.UtcNow.AddMinutes(-30);
                    var unverifiedUsers = await userRepository.GetUnverifiedOlderThanAsync(cutoffTime);

                    foreach (var user in unverifiedUsers)
                    {
                        _logger.LogInformation("Удаление неподтвержденного пользователя: {Email}", user.Email);
                        await userRepository.DeleteAsync(user.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при очистке неподтвержденных пользователей.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("UnverifiedUserCleanupService остановлен.");
        }
    }
}

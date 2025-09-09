using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class BonusTransactionApprovalService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<BonusTransactionApprovalService> _logger;

    public BonusTransactionApprovalService(IServiceProvider provider, ILogger<BonusTransactionApprovalService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            int successCount = 0;
            int failureCount = 0;
            using var scope = _provider.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
            var sellerService = scope.ServiceProvider.GetRequiredService<ISellerBffService>();

            var pending = await dataService.GetPendingBonusTransactionsAsync();

            foreach (var transaction in pending)
            {
                if (transaction.UserId != null)
                {
                    try
                    {
                        var success = await scope.ServiceProvider.GetRequiredService<IBuyerBffService>()
                            .ConfirmPendingBonusTransactionAsync((Guid)transaction.UserId, transaction.Id);

                        if (!success)
                        {
                            _logger.LogWarning(
                                "Failed to process bonus confirmation for transaction {TransactionId}",
                                transaction.Id);
                            failureCount++;
                        }
                        else
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(
                            ex,
                            "Unhandled exception while processing bonus confirmation {TransactionId}",
                            transaction.Id);
                    }
                }
            }
            _logger.LogInformation(
                "Cashback batch summary: {SuccessCount} succeeded, {FailureCount} failed at {UtcNow}",
                successCount, failureCount, DateTime.UtcNow);
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        }
    }
}
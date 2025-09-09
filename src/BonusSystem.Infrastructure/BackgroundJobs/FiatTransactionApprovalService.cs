using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class FiatTransactionApprovalService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<FiatTransactionApprovalService> _logger;

    public FiatTransactionApprovalService(IServiceProvider provider, ILogger<FiatTransactionApprovalService> logger)
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

            var pending = await dataService.FiatTransactions.GetPendingFiatTransactionsAsync(FiatTransactionStatus.Pending);

            foreach (var transaction in pending)
            {
                if (transaction.UserId != null && transaction.SellerId != null)
                {
                    try
                    {
                        var request = new FiatCashbackRequestDto
                        {
                            FiatTransactionId = transaction.Id,
                            BuyerId = (Guid)transaction.UserId,
                            SellerId = (Guid)transaction.SellerId,
                            FiatTransaction = transaction,
                        };

                        var result = await sellerService.ProcessCashBackTransactionAsync(request);

                        if (!result.Success)
                        {
                            _logger.LogWarning($"Failed to process cashback for transaction {transaction.Id}: {result.ErrorMessage}");
                            failureCount++;
                        }
                        else successCount++;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex,
                            $"Unhandled exception while processing cashBack transaction {transaction.Id}");
                    }

                }
            }
            _logger.LogInformation($"Cashback batch summary: {successCount} succeeded, {failureCount} failed at {DateTime.UtcNow}");
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);


        }
    }
}
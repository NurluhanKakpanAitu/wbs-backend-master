using System;
using System.Data;
using System.Threading.Tasks;
using BonusSystem.Application.Common.Transactions;
using BonusSystem.Core.Exceptions;
using BonusSystem.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;

public class TransactionExecutor : ITransactionExecutor
{
    private readonly IDataService _dataService;
    private readonly ILogger<TransactionExecutor> _logger;

    private readonly AsyncPolicy _retryPolicy;

    public TransactionExecutor(IDataService dataService, ILogger<TransactionExecutor> logger)
    {
        _dataService = dataService;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<ConcurrencyException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Retry {RetryCount} due to concurrency exception", retryCount);
                });
    }

    public async Task ExecuteWithRetryAsync(Func<Task> operation)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _dataService.ExecuteInTransactionAsync(operation, IsolationLevel.Serializable);
        });
    }
}

namespace BonusSystem.Application.Common.Transactions;

public interface ITransactionExecutor
{
    Task ExecuteWithRetryAsync(Func<Task> operation);
}

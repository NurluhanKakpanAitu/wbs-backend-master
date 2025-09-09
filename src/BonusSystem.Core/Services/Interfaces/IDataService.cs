using System.Data;
using BonusSystem.Core.Repositories;
using BonusSystem.Shared.Dtos; 
namespace BonusSystem.Core.Services.Interfaces;

/// <summary>
/// Service that provides access to all repositories
/// </summary>
public interface IDataService
{
    IUserRepository Users { get; }
    ICompanyRepository Companies { get; }
    IStoreRepository Stores { get; }
    ICategoryRepository Categories { get; }
    ITransactionRepository Transactions { get; }
    IFiatTransactionRepository FiatTransactions { get; }
    ITransferRepository Transfers{ get; }
    INotificationRepository Notifications { get; } 
    IMallRepository Malls { get; } 
    IFiatReplenishmentCompanyBalanceRepository FiatReplenishmentCompanyBalances { get; }

    ITransactionReturnRepository TransactionReturns { get; }

    Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.Serializable);
    Task<IReadOnlyList<FiatTransactionDto>> GetPendingFiatTransactionsAsync();
    Task<IReadOnlyList<TransactionDto>> GetPendingBonusTransactionsAsync();
}
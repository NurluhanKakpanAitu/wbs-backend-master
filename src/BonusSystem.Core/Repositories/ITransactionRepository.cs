using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Repositories;

/// <summary>
/// Repository for transaction-related operations
/// </summary>
public interface ITransactionRepository : IRepository<TransactionDto, Guid>
{
    Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAsync(Guid userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByStoreIdAsync(Guid storeId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByTypeAsync(TransactionType type);
    Task<bool> UpdateTransactionStatusAsync(Guid transactionId, TransactionStatus status);
    Task<IEnumerable<TransactionDto>> GetTransactionsInDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalBonusCirculationAsync();
    Task<decimal> GetTotalActiveBonus();
    Task<int> GetTotalTransactionsCountAsync(); 
    IQueryable<TransactionDto> GetTransactionsByUserIdQuery(Guid userId);
    Task<CommissionProcessingResult> ProcessCompanyCommissionsAsync(); 

    // For bonus expiration
    Task<IEnumerable<TransactionDto>> GetActiveTransactionsForUserAsync(Guid userId);
    Task<IEnumerable<TransactionDto>> GetActiveTransactionsForCompanyAsync(Guid companyId);
    Task ProcessExpirationAsync(DateTime expirationDate);
    Task<IReadOnlyList<TransactionDto>> GetPendingAsync();

}
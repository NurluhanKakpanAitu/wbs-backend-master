using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Repositories;

/// <summary>
/// Repository for fiat transaction-related operations
/// </summary>
public interface IFiatTransactionRepository : IRepository<FiatTransactionDto, Guid>
{
    Task<IEnumerable<FiatTransactionDto>> GetTransactionsByUserIdAsync(Guid userId);
    Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsByUserIdAsync(Guid userId);
    Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsByStoreIdAsync(Guid storeId);
    Task<bool> UpdateFiatTransactionStatusAsync(Guid transactionId, FiatTransactionStatus status);
    Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsInDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> GetTotalFiatTransactionsCountAsync();
    IQueryable<FiatTransactionDto> GetTransactionsByUserIdQuery(Guid userId);
    Task<IEnumerable<FiatTransactionDto>> GetPendingFiatTransactionsAsync(FiatTransactionStatus status);
    
    Task<IEnumerable<FiatTransactionDto>> GetActiveFiatTransactionsForUserAsync(Guid userId);
    Task<IEnumerable<FiatTransactionDto>> GetActiveFiatTransactionsForCompanyAsync(Guid companyId);
    Task CreatePendingTransactionAsync(FiatTransactionDto transaction);
}
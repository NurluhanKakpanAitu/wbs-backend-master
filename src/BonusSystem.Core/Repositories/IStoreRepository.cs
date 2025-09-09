using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Repositories;

/// <summary>
/// Repository for store-related operations
/// </summary>
public interface IStoreRepository : IRepository<StoreDto, Guid>
{
    Task<int> GetCountAsync(int categoryId);
    Task<List<StoreDto>> GetPagedAsync(int categoryId, int page, int pageSize);
    Task<IEnumerable<StoreDto>> GetStoresByCompanyIdAsync(Guid companyId);
    Task<int> GetStoresCountByCompanyIdAsync(Guid companyId);
    Task<PagedResult<StoreDto>> GetStoresByCompanyIdPagedAsync(Guid companyId, int page, int pageSize);
    Task<bool> UpdateStatusAsync(Guid storeId, StoreStatus status);
    Task<bool> RemoveSellerFromStoreAsync(Guid storeId, Guid sellerId);
    Task<IEnumerable<UserDto>> GetSellersByStoreIdAsync(Guid storeId);
    Task<IEnumerable<StoreDto>> GetStoresByCategoryAsync(string category);
    Task<StoreDto?> GetStoreBySellerIdAsync(Guid sellerId);
    Task<decimal> GetStoreBonusBalanceAsync(Guid storeId);
    Task<decimal> GetStoreFiatBalanceAsync(Guid storeId);  
    Task<PagedResult<CombinedTransactionDto>> AllTransactionsForStoreAsync(Guid storeId, int page, int pageSize);
}
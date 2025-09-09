using BonusSystem.Shared.Dtos;

namespace BonusSystem.Core.Services.Interfaces;

public interface ISellerBffService : IBaseBffService
{
    Task<PagedResult<CombinedTransactionDto>> GetBuyerTransactionsByFrontendIdAsync(Guid sellerId, string frontendId, int page, int pageSize); 
    Task<TransactionResultDto> ProcessTransactionAsync(Guid sellerId, TransactionRequestDto request);
    Task<FiatTransactionResultDto> ProcessFiatTransactionAsync(Guid sellerId, FiatTransactionRequestDto request);
    Task<CombinedTransactionResultDto> ProcessCombinedTransactionAsync(Guid sellerId, CombinedTransactionRequestDto request);
    Task<FiatTransactionResultDto> ProcessCashBackTransactionAsync(FiatCashbackRequestDto request);
    Task<BonusBalanceDto> GetBuyerBonusBalanceAsync(string buyerId);
    Task<decimal> GetStoreBonusBalanceAsync(Guid storeId);
    Task<decimal> GetStoreFiatBalanceAsync(Guid storeId);
    Task<decimal> GetStoreBonusBalanceByUserIdAsync(Guid userId);
    Task<CompanyDto> GetCompanyForSeller(Guid userId);
    Task<IEnumerable<StoreBonusTransactionsDto>> GetStoreBonusTransactionsAsync(Guid storeId);
    Task<IEnumerable<StoreBonusTransactionsDto>> GetStoreBonusTransactionsByUserIdAsync(Guid userId);
    Task<TransactionReturnResultDto> RequestTransactionReturnAsync(Guid sellerId, TransactionReturnRequestDto request);
    Task<IEnumerable<TransactionReturnDto>> GetTransactionReturnsRequestedBySellerAsync(Guid sellerId);
    Task<IEnumerable<StoreFiatTransactionsDto>> GetStoreFiatTransactionsByUserIdAsync(Guid userId);
    Task<FiatTransactionResultDto> CreatePendingFiatTransactionAsync(Guid sellerId, FiatTransactionRequestDto request);
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId);
    Task<PagedResult<CombinedTransactionDto>> GetTransactionsForSeller(Guid sellerId, int page, int pageSize); 
}
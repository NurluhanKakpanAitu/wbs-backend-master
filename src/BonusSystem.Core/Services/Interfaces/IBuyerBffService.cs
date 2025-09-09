using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;


namespace BonusSystem.Core.Services.Interfaces;

public interface IBuyerBffService : IBaseBffService
{
    Task<PagedResult<CompanyStoresDto>> FindStoresAsync(StoreFilterRequestDto filter);
    Task<TransferResultDto> ReplenishWalletAsync(Guid userId, decimal amount); 
    Task<PagedResult<TransferHistoryDto>> GetTransfersAsync(Guid userId, int page, int pageSize); 
    Task<TransferResultDto> TransferBonusesAsync(Guid senderId, TransferForm request);
    Task<PagedResult<TransferHistoryDto>> GetReplenishmentsForBuyerAsync(Guid buyerId, int page, int pageSize); 
    Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page, int pageSize);
    // Task<TransactionDto> GetAllTransactionsAsync(Guid userId);
    // Task<UserDto> GetUserByFrontendIdAsync(string id);
    Task<BonusTransactionSummaryDto> GetBonusSummaryAsync(Guid userId);
    Task<FiatBalanceDto> GetFiatBalanceAsync(Guid userId);
    Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid userId);
    Task<string> GenerateQrCodeAsync(Guid userId);
    Task<IEnumerable<StoreDto>> FindStoresByCategoryAsync(string category);
    Task<bool> ApproveBonusTransactionReturnAsync(Guid buyerId, Guid transactionId);
    Task<bool> ApproveFiatTransactionReturnAsync(Guid buyerId, Guid fiatTransactionId);
    Task<IEnumerable<TransactionReturnDto>> GetApprovedReturnsByUserAsync(Guid buyerId);
    Task<IEnumerable<TransactionReturnDto>> GetPendingReturnsToApproveAsync(Guid buyerId);
    Task<bool> RejectTransactionReturnAsync(Guid userId, Guid returnId);
    Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionHistoryAsync(Guid userId);
    Task<bool> ConfirmPendingFiatTransactionAsync(Guid buyerId, Guid fiatTransactionId);
    Task<bool> ConfirmPendingBonusTransactionAsync(Guid buyerId, Guid transactionId);
    Task<DateTime?> GetDateBonusRemove(Guid userId);
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId);
    Task<PagedResult<CombinedTransactionDto>> AllTransactionsForUser(Guid userId, int page = 1, int pageSize = 20);
}
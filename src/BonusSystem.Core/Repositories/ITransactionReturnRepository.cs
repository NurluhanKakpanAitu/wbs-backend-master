using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Repositories;

public interface ITransactionReturnRepository
{
    Task<TransactionReturnDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(TransactionReturnDto dto);
    Task<bool> UpdateAsync(TransactionReturnDto dto);
    Task<IEnumerable<TransactionReturnDto>> GetByStatusAsync(TransactionReturnStatus status);

    Task<IEnumerable<TransactionReturnDto>> GetByRequestedUserIdAsync(Guid userId, TransactionReturnStatus? status = null);
    Task<IEnumerable<TransactionReturnDto>> GetByApprovedUserIdAsync(Guid userId);
    Task<TransactionReturnDto?> GetByBonusTransactionIdAsync(Guid transactionId, TransactionReturnStatus? status = null);
    Task<TransactionReturnDto?> GetByFiatTransactionIdAsync(Guid transactionId, TransactionReturnStatus? status = null);
    Task<IEnumerable<TransactionReturnDto>> GetPendingReturnsByBuyerIdAsync(Guid buyerId);
}

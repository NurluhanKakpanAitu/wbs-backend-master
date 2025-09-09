using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Repositories;

/// <summary>
/// Repository for user-related operations
/// </summary>
public interface IUserRepository : IRepository<UserDto, Guid>
{
    Task<int> RemoveAllBonusesFromAllUsersAsync();
    Task<bool> UpdatePasswordAsync(Guid userid, string new_password);
    Task<bool> UpdatePincodeStatus(Guid userId);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<List<UserDto>> GetUnverifiedOlderThanAsync(DateTime cutoff);
    Task<bool> UpdateBalanceAsync(Guid userId, decimal newBalance, decimal expectedCurrentBalance);
    Task<bool> UpdateFiatBalanceAsync(Guid userId, decimal newFiatBalance, decimal expectedFiatBalance);
    Task<UserRole> GetUserRoleAsync(Guid userId);
    Task<bool> IsUserExistsByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role);
    Task<UserDto?> GetByFrontendIdAsync(string frontendId);
    Task<PagedResult<TransferHistoryDto>> GetTransfersForUserAsync(Guid userId, int page, int pageSize);
    Task<PagedResult<TransferHistoryDto>> GetReplenishmentsAsync(Guid userId, int page, int pageSize); 

    // Company relationship methods
    Task<IEnumerable<UserDto>> GetUsersByCompanyIdAsync(Guid companyId);
    Task<bool> AssignUserToCompanyAsync(Guid userId, Guid companyId);
    Task<UserDto?> GetCompanyAdminUserByCompanyIdAsync(Guid companyId);

    // Email verification methods
    Task<bool> SetEmailVerifiedAsync(string email);
    Task<bool> SetVerificationCodeAsync(string email, string code);
    Task<int> GetUserCountAsync();
    Task<List<UserDto>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> UpdateStoreAssignmentAsync(Guid userId, Guid? storeId);
    Task<PagedResult<CombinedTransactionDto>> AllTransactionsForUserAsync(Guid userId, int page, int pageSize);

    // Task<PagedResult<CombinedTransactionDto>> AllTransactionsForUserForStoreAsync(Guid userId, int page, int pageSize); 
    Task<IEnumerable<UserDto>> GetUsersByStoreIdAsync(Guid storeId);

}
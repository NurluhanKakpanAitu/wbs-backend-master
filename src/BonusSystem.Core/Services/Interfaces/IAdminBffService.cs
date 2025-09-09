using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Services.Interfaces;

public interface IAdminBffService : IBaseBffService
{
    Task<CompanyRegistrationResultDto> RegisterCompanyAsync(CompanyRegistrationDto request);
    Task<(bool, string)> AppointAdminAsync(AppointAdminDto request);
    Task<UserDto> FindUserAsync(string email); 
    Task<UserDto> FindUserByFrontendIdAsync(string frontendId);
    // Task<CompanyRegistrationResultDto> RegisterCompanyWithAdminAsync(CompanyWithAdminRegistrationDto request);
    Task<bool> UpdateCompanyStatusAsync(Guid companyId, CompanyStatus status);
    Task<bool> ModerateStoreAsync(Guid storeId, bool isApproved);
    Task<bool> CreditCompanyBalanceAsync(Guid companyId, decimal amount); 
    Task<bool> ReplenishFiatCompanyAsync(Guid companyId, decimal amount); 
    Task<AuthResult> SendCompanyURLAsync(Guid companyId); 
    Task<IEnumerable<TransactionDto>> GetSystemTransactionsAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> SendSystemNotificationAsync(Guid? recipientId, string message, NotificationType type);
    Task<List<CompanyFeeResult>> GetTransactionFeesAsync(TransactionFeeRequest request);
    Task<IEnumerable<FiatTransactionDto>> GetSystemFiatTransactionsAsync(Guid? companyId = null, DateTime? startDate = null, DateTime? endDate = null);
}
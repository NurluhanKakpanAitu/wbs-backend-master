using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Repositories;

/// <summary>
/// Repository for company-related operations
/// </summary>
public interface ICompanyRepository : IRepository<CompanyDto, Guid>
{
    Task<CompanyDto?> FindContractAsync(string bin, string Number_of_contract, string inn, string name);
    Task<bool> UpdateBalanceAsync(Guid companyId, decimal newBalance, decimal expectedCurrentBalance);
    Task<bool> UpdateFiatBalanceAsync(Guid companyId, decimal newFiatBalance, decimal expectedCurrentFiatBalance);
    Task<bool> CreditBalanceAsync(Guid companyId, decimal amount);
    Task<CompanyDto?> GetByBinAsync(string bin);
    Task<CompanyDto?> GetBySellerId(Guid userId); 
    Task<CompanyDto?> GetByNumber_of_contractAsync(string Number_of_contract);
    Task<CompanyDto?> GetByINNAsync(string inn);
    Task<CompanyDto?> GetByUserNameAsync(string companyName);
    Task<bool> UpdateStatusAsync(Guid companyId, CompanyStatus status);
    Task<IEnumerable<CompanyDto>> GetCompaniesByStatusAsync(CompanyStatus status);
    Task<decimal> GetOriginalBalanceAsync(Guid companyId);
    Task<bool> ResetToOriginalBalanceAsync(Guid companyId);
    Task<IEnumerable<CompanySummaryDto>> GetCompanySummariesAsync();
    Task<bool> CreditFiatBalanceAsync(Guid companyId, decimal amount); 
    Task<List<Guid>> GetSellersForCompanyAsync(Guid companyId); 
}
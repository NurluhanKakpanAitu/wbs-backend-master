using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Services.Implementations.BFF;

/// <summary>
/// BFF service for Observer role
/// </summary>
public class ObserverBffService : BaseBffService, IObserverBffService
{
    public ObserverBffService(
        IDataService dataService,
        IAuthenticationService authService) 
        : base(dataService, authService)
    {
    }

    /// <summary>
    /// Gets the permitted actions for an observer
    /// </summary>
    public override async Task<IEnumerable<PermittedActionDto>> GetPermittedActionsAsync(Guid userId)
    {
        var role = await _dataService.Users.GetUserRoleAsync(userId);
        if (role != UserRole.CompanyObserver && role != UserRole.SystemObserver)
        {
            return Enumerable.Empty<PermittedActionDto>();
        }

        var actions = new List<PermittedActionDto>
        {
            new() { ActionName = "GetStatistics", Description = "Get system statistics", Endpoint = "/api/observers/statistics" },
            new() { ActionName = "GetTransactionSummary", Description = "Get transaction summary", Endpoint = "/api/observers/transactions/summary" },
        };

        // System observers can see all companies
        if (role == UserRole.SystemObserver)
        {
            actions.Add(new PermittedActionDto { ActionName = "GetCompaniesOverview", Description = "Get companies overview", Endpoint = "/api/observers/companies" });
        }

        return actions;
    }

    /// <summary>
    /// Gets dashboard statistics
    /// </summary>
    public async Task<DashboardStatisticsDto> GetStatisticsAsync(StatisticsQueryDto query)
    {
        // Total bonus circulation
        var totalBonusCirculation = await _dataService.Transactions.GetTotalBonusCirculationAsync();

        // Current active bonus
        var currentActiveBonus = await _dataService.Transactions.GetTotalActiveBonus();

        // Total transactions count
        var totalTransactions = await _dataService.Transactions.GetTotalTransactionsCountAsync();

        // Active users count (simplification: all buyer users)
        var buyerUsers = await _dataService.Users.GetUsersByRoleAsync(UserRole.Buyer);
        var activeUsers = buyerUsers.Count();

        // Active companies count
        var activeCompanies = (await _dataService.Companies.GetCompaniesByStatusAsync(CompanyStatus.Active)).Count();

        // Active stores count
        var allStores = await _dataService.Stores.GetAllAsync();
        var activeStores = allStores.Count(s => s.Status == StoreStatus.Active);

        // Filter data by CompanyId if provided
        if (query.CompanyId.HasValue)
        {
            var companyTransactions = await _dataService.Transactions.GetTransactionsByCompanyIdAsync(query.CompanyId.Value);
            totalBonusCirculation = companyTransactions.Sum(t => t.Type == TransactionType.Earn ? t.BonusAmount : -t.BonusAmount);
            totalTransactions = companyTransactions.Count();

            var companyStores = await _dataService.Stores.GetStoresByCompanyIdAsync(query.CompanyId.Value);
            activeStores = companyStores.Count(s => s.Status == StoreStatus.Active);
        }

        return new DashboardStatisticsDto
        {
            TotalBonusCirculation = totalBonusCirculation,
            CurrentActiveBonus = currentActiveBonus,
            TotalTransactions = totalTransactions,
            ActiveUsers = activeUsers,
            ActiveCompanies = activeCompanies,
            ActiveStores = activeStores
        };
    }

    /// <summary>
    /// Gets transaction summary
    /// </summary>
    public async Task<TransactionDto> GetTransactionSummaryAsync(Guid? companyId)
    {
        // This is a simplified implementation for the prototype
        // In a real application, this would return a more detailed summary

        IEnumerable<TransactionDto> transactions;

        if (companyId.HasValue)
        {
            transactions = await _dataService.Transactions.GetTransactionsByCompanyIdAsync(companyId.Value);
        }
        else
        {
            transactions = await _dataService.Transactions.GetAllAsync();
        }

        // Create a dummy transaction to represent the summary
        return new TransactionDto
        {
            Id = Guid.Empty,
            BonusAmount = transactions.Sum(t => t.Type == TransactionType.Earn ? t.BonusAmount : -t.BonusAmount),
            Type = TransactionType.AdminAdjustment,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed,
            Description = "Transaction Summary"
        };
    }

    /// <summary>
    /// Gets companies overview
    /// </summary>
    public async Task<IEnumerable<CompanySummaryDto>> GetCompaniesOverviewAsync()
    {
        return await _dataService.Companies.GetCompanySummariesAsync();
    }
}

using BonusSystem.Shared.Dtos;

namespace BonusSystem.Core.Services.Interfaces;

public record StatisticsQueryDto
{
    public Guid? CompanyId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
public record MonitoringDto
{
    public Guid CompanyId { get; init; }
    public DateTime Date { get; init; }
    public decimal BonusBalance { get; init; }
    public decimal GivenBonuses { get; init; }
    public decimal ReceivedBonuses { get; init; }
    public decimal Sales { get; init; }
    public decimal Commission { get; init; }
    public List<DashBoardStoreStatisticsDto> StoreMetrics { get; init; } = new();
} 

public class QuarterlyStatsDto
{
    public string Period { get; set; } = null!; 
    public decimal BonusBalance { get; set; }
    public decimal WalletBalance { get; set; }
    public decimal SalesAmount { get; set; }
    public int DealCount { get; set; }
    public int RefundCount { get; set; }
    public decimal CashbackAmount { get; set; }
    public decimal Commission { get; set; }
    public string CommissionPaymentStatus { get; set; } = null!;
}

public record DashboardStatisticsDto
{
    public decimal TotalBonusCirculation { get; init; }
    public decimal CurrentActiveBonus { get; init; }
    public decimal CurrentActiveFiat { get; init; }
    public int TotalTransactions { get; init; }
    public int TotalFiatTransactions { get; init; }

    public int ActiveUsers { get; init; }
    public int ActiveCompanies { get; init; }
    public int ActiveStores { get; init; }
}
public class PagedStoreStatisticsDto
{
    public List<DashBoardStoreStatisticsDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
public record DashBoardStoreStatisticsDto
{
    public Guid StoreId { get; init; }
    public DateTime Date { get; init; }
    public Guid ClientId { get; init; }
    public string OperationForBonusAccount { get; init; }
    public decimal BonusGiven { get; init; }
    public decimal BonusGetting { get; init; }
    public decimal SellingBonus { get; init; }
    public decimal Commission { get; init; }
} 

public interface IObserverBffService : IBaseBffService
{
    Task<DashboardStatisticsDto> GetStatisticsAsync(StatisticsQueryDto query);
    Task<TransactionDto> GetTransactionSummaryAsync(Guid? companyId);
    Task<IEnumerable<CompanySummaryDto>> GetCompaniesOverviewAsync();
}
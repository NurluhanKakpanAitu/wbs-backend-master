using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BonusSystem.Core.Services.Implementations;

public class RealTimeMonitoringService : IRealTimeMonitoringService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IDataService _dataService;
    private readonly ILogger<RealTimeMonitoringService> _logger;
    
    // In-memory storage for active monitoring sessions
    private readonly ConcurrentDictionary<Guid, Timer> _activeMonitoringSessions = new();
    private readonly ConcurrentDictionary<Guid, CompanyRealTimeStatisticsDto> _cachedStatistics = new();

    public RealTimeMonitoringService(
        ICompanyRepository companyRepository,
        ITransactionRepository transactionRepository,
        IStoreRepository storeRepository,
        IDataService dataService,
        ILogger<RealTimeMonitoringService> logger)
    {
        _companyRepository = companyRepository;
        _transactionRepository = transactionRepository;
        _storeRepository = storeRepository;
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<CompanyRealTimeStatisticsDto> GetCurrentStatisticsAsync(Guid companyId)
    {
        if (_cachedStatistics.TryGetValue(companyId, out var cachedStats))
        {
            // Return cached stats if they're recent (less than 5 minutes old)
            if (DateTime.UtcNow.Subtract(cachedStats.LastUpdated).TotalMinutes < 5)
            {
                return cachedStats;
            }
        }

        var stats = await CalculateRealTimeStatisticsAsync(companyId, DateTime.UtcNow);
        _cachedStatistics.AddOrUpdate(companyId, stats, (key, oldValue) => stats);
        
        return stats;
    }

    public async Task<CompanyRealTimeStatisticsDto> GetStatisticsForDateAsync(Guid companyId, DateTime date)
    {
        return await CalculateRealTimeStatisticsAsync(companyId, date);
    }

    public async Task<CompanyQuarterlyStatisticsDto> GetQuarterlyStatisticsAsync(Guid companyId, int year, int quarter)
    {
        var startDate = GetQuarterStartDate(year, quarter);
        var endDate = GetQuarterEndDate(year, quarter);
        
        var dailyStats = new List<CompanyDailyStatisticsDto>();
        var currentDate = startDate;
        
        while (currentDate <= endDate)
        {
            var dailyStat = await GetDailyStatisticsAsync(companyId, currentDate);
            dailyStats.Add(dailyStat);
            currentDate = currentDate.AddDays(1);
        }

        var totalBonusBalance = dailyStats.Sum(d => d.BonusBalance);
        var totalWalletBalance = dailyStats.Sum(d => d.WalletBalance);
        var totalSalesAmount = dailyStats.Sum(d => d.SalesAmount);
        var totalDealCount = dailyStats.Sum(d => d.DealCount);
        var totalRefundCount = dailyStats.Sum(d => d.RefundCount);
        var totalCashbackAmount = dailyStats.Sum(d => d.CashbackAmount);
        var totalCommission = dailyStats.Sum(d => d.Commission);

        var totalStats = new CompanyQuarterlyStatisticsDto
        {
            CompanyId = companyId,
            Year = year,
            Quarter = quarter,
            TotalBonusBalance = totalBonusBalance,
            TotalWalletBalance = totalWalletBalance,
            TotalSalesAmount = totalSalesAmount,
            TotalDealCount = totalDealCount,
            TotalRefundCount = totalRefundCount,
            TotalCashbackAmount = totalCashbackAmount,
            TotalCommission = totalCommission,
            DailyStatistics = dailyStats
        };

        // Calculate commission payment status for the quarter
        var commissionPaymentStatus = await CalculateCommissionPaymentStatusAsync(companyId, endDate) > 0 
            ? "Paid" 
            : "Pending";

        totalStats = totalStats with { CommissionPaymentStatus = commissionPaymentStatus };

        return totalStats;
    }

    public async Task<List<CompanyDailyStatisticsDto>> GetMonthlyStatisticsAsync(Guid companyId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var dailyStats = new List<CompanyDailyStatisticsDto>();
        var currentDate = startDate;
        
        while (currentDate <= endDate)
        {
            var dailyStat = await GetDailyStatisticsAsync(companyId, currentDate);
            dailyStats.Add(dailyStat);
            currentDate = currentDate.AddDays(1);
        }

        return dailyStats;
    }

    public async Task<CompanyRealTimeStatisticsDto> GetStatisticsAtDateAsync(Guid companyId, DateTime date)
    {
        return await CalculateRealTimeStatisticsAsync(companyId, date);
    }

    public async Task<decimal> CalculateCommissionPaymentStatusAsync(Guid companyId, DateTime date)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
            return 0;

        var transactions = (await _transactionRepository.GetTransactionsByCompanyIdAsync(companyId))
            .Where(t => t.Timestamp.Date <= date.Date)
            .ToList();

        var totalCommission = transactions.Sum(t => (t.CommissionPercent / 100m) * t.TotalCost);
        
        // For now, assume commission is paid if it's less than a certain threshold
        // In a real system, you'd check actual payment records
        return totalCommission;
    }

    public Task StartRealTimeMonitoringAsync(Guid companyId)
    {
        if (_activeMonitoringSessions.ContainsKey(companyId))
        {
            _logger.LogWarning("Monitoring already active for company {CompanyId}", companyId);
            return Task.CompletedTask;
        }

        var timer = new Timer(async _ => await UpdateStatisticsAsync(companyId), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        _activeMonitoringSessions.TryAdd(companyId, timer);
        
        _logger.LogInformation("Started real-time monitoring for company {CompanyId}", companyId);
        return Task.CompletedTask;
    }

    public Task StopRealTimeMonitoringAsync(Guid companyId)
    {
        if (_activeMonitoringSessions.TryRemove(companyId, out var timer))
        {
            timer?.Dispose();
            _cachedStatistics.TryRemove(companyId, out _);
            _logger.LogInformation("Stopped real-time monitoring for company {CompanyId}", companyId);
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsMonitoringActiveAsync(Guid companyId)
    {
        return Task.FromResult(_activeMonitoringSessions.ContainsKey(companyId));
    }

    private async Task<CompanyRealTimeStatisticsDto> CalculateRealTimeStatisticsAsync(Guid companyId, DateTime date)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
            throw new ArgumentException($"Company with ID {companyId} not found");

        var transactions = (await _transactionRepository.GetTransactionsByCompanyIdAsync(companyId))
            .Where(t => t.Timestamp.Date == date.Date)
            .ToList();

        var fiatTransactions = (await _dataService.FiatTransactions.GetFiatTransactionsByCompanyIdAsync(companyId))
            .Where(t => t.Timestamp.Date == date.Date)
            .ToList();

        var stores = await _storeRepository.GetStoresByCompanyIdAsync(companyId);

        var salesAmount = transactions.Sum(t => t.TotalCost);
        var dealCount = transactions.Count;
        var refundCount = transactions.Count(t => t.Status == Shared.Models.TransactionStatus.Reversed);
        var cashbackAmount = fiatTransactions.Sum(t => t.FiatCashBackAmount);
        var commission = transactions.Sum(t => (t.CommissionPercent / 100m) * t.TotalCost);

        var commissionPaymentStatus = await CalculateCommissionPaymentStatusAsync(companyId, date) > 0 ? "Paid" : "Pending";

        return new CompanyRealTimeStatisticsDto
        {
            CompanyId = companyId,
            Date = date,
            BonusBalance = Math.Round(company.BonusBalance, 2),
            WalletBalance = Math.Round(company.FiatBalance, 2),
            SalesAmount = Math.Round(salesAmount, 2),
            DealCount = dealCount,
            RefundCount = refundCount,
            CashbackAmount = Math.Round(cashbackAmount, 2),
            Commission = Math.Round(commission, 2),
            CommissionPaymentStatus = commissionPaymentStatus,
            LastUpdated = DateTime.UtcNow
        };
    }

    private async Task<CompanyDailyStatisticsDto> GetDailyStatisticsAsync(Guid companyId, DateTime date)
    {
        var baseStats = await CalculateRealTimeStatisticsAsync(companyId, date);
        var stores = await _storeRepository.GetStoresByCompanyIdAsync(companyId);
        
        var storeStats = new List<StoreDailyStatisticsDto>();
        
        foreach (var store in stores)
        {
            var storeTransactions = (await _transactionRepository.GetTransactionsByStoreIdAsync(store.Id))
                .Where(t => t.Timestamp.Date == date.Date)
                .ToList();

            var storeSalesAmount = storeTransactions.Sum(t => t.TotalCost);
            var storeDealCount = storeTransactions.Count;
            var storeRefundCount = storeTransactions.Count(t => t.Status == Shared.Models.TransactionStatus.Reversed);
            var storeCommission = storeTransactions.Sum(t => (t.CommissionPercent / 100m) * t.TotalCost);

            storeStats.Add(new StoreDailyStatisticsDto
            {
                StoreId = store.Id,
                StoreName = store.Name,
                SalesAmount = Math.Round(storeSalesAmount, 2),
                DealCount = storeDealCount,
                RefundCount = storeRefundCount,
                Commission = Math.Round(storeCommission, 2)
            });
        }

        return new CompanyDailyStatisticsDto
        {
            CompanyId = companyId,
            Date = date,
            BonusBalance = baseStats.BonusBalance,
            WalletBalance = baseStats.WalletBalance,
            SalesAmount = baseStats.SalesAmount,
            DealCount = baseStats.DealCount,
            RefundCount = baseStats.RefundCount,
            CashbackAmount = baseStats.CashbackAmount,
            Commission = baseStats.Commission,
            CommissionPaymentStatus = baseStats.CommissionPaymentStatus,
            StoreStatistics = storeStats
        };
    }

    private async Task UpdateStatisticsAsync(Guid companyId)
    {
        try
        {
            var stats = await CalculateRealTimeStatisticsAsync(companyId, DateTime.UtcNow);
            _cachedStatistics.AddOrUpdate(companyId, stats, (key, oldValue) => stats);
            
            // Here you would typically notify connected clients via SignalR
            _logger.LogDebug("Updated statistics for company {CompanyId}: Sales: {SalesAmount}, Deals: {DealCount}", 
                companyId, stats.SalesAmount, stats.DealCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating statistics for company {CompanyId}", companyId);
        }
    }

    private DateTime GetQuarterStartDate(int year, int quarter)
    {
        return quarter switch
        {
            1 => new DateTime(year, 1, 1),
            2 => new DateTime(year, 4, 1),
            3 => new DateTime(year, 7, 1),
            4 => new DateTime(year, 10, 1),
            _ => throw new ArgumentException("Quarter must be between 1 and 4")
        };
    }

    private DateTime GetQuarterEndDate(int year, int quarter)
    {
        return quarter switch
        {
            1 => new DateTime(year, 3, 31),
            2 => new DateTime(year, 6, 30),
            3 => new DateTime(year, 9, 30),
            4 => new DateTime(year, 12, 31),
            _ => throw new ArgumentException("Quarter must be between 1 and 4")
        };
    }
}

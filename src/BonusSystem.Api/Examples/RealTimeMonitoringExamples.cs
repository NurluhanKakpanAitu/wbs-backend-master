using BonusSystem.Shared.Dtos;

namespace BonusSystem.Api.Examples;

public static class RealTimeMonitoringExamples
{
    public static CompanyRealTimeStatisticsDto GetRealTimeStatisticsExample()
    {
        return new CompanyRealTimeStatisticsDto
        {
            CompanyId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            BonusBalance = 15000.50m,
            WalletBalance = 25000.75m,
            SalesAmount = 125000.00m,
            DealCount = 45,
            RefundCount = 2,
            CashbackAmount = 1250.00m,
            Commission = 6250.00m,
            CommissionPaymentStatus = "Paid",
            LastUpdated = DateTime.UtcNow
        };
    }

    public static CompanyDailyStatisticsDto GetDailyStatisticsExample()
    {
        return new CompanyDailyStatisticsDto
        {
            CompanyId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            BonusBalance = 15000.50m,
            WalletBalance = 25000.75m,
            SalesAmount = 125000.00m,
            DealCount = 45,
            RefundCount = 2,
            CashbackAmount = 1250.00m,
            Commission = 6250.00m,
            CommissionPaymentStatus = "Paid",
            StoreStatistics = new List<StoreDailyStatisticsDto>
            {
                new StoreDailyStatisticsDto
                {
                    StoreId = Guid.NewGuid(),
                    StoreName = "Main Store",
                    SalesAmount = 75000.00m,
                    DealCount = 25,
                    RefundCount = 1,
                    Commission = 3750.00m
                },
                new StoreDailyStatisticsDto
                {
                    StoreId = Guid.NewGuid(),
                    StoreName = "Branch Store",
                    SalesAmount = 50000.00m,
                    DealCount = 20,
                    RefundCount = 1,
                    Commission = 2500.00m
                }
            }
        };
    }

    public static CompanyQuarterlyStatisticsDto GetQuarterlyStatisticsExample()
    {
        return new CompanyQuarterlyStatisticsDto
        {
            CompanyId = Guid.NewGuid(),
            Year = 2025,
            Quarter = 2,
            TotalBonusBalance = 15000.50m,
            TotalWalletBalance = 25000.75m,
            TotalSalesAmount = 3750000.00m,
            TotalDealCount = 1350,
            TotalRefundCount = 45,
            TotalCashbackAmount = 37500.00m,
            TotalCommission = 187500.00m,
            CommissionPaymentStatus = "Paid",
            DailyStatistics = new List<CompanyDailyStatisticsDto>
            {
                GetDailyStatisticsExample(),
                GetDailyStatisticsExample()
            }
        };
    }

    public static RealTimeStatisticsUpdateDto GetStatisticsUpdateExample()
    {
        return new RealTimeStatisticsUpdateDto
        {
            CompanyId = Guid.NewGuid(),
            Statistics = GetRealTimeStatisticsExample(),
            UpdateType = "realtime",
            Timestamp = DateTime.UtcNow
        };
    }
}

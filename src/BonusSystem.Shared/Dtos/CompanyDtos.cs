using BonusSystem.Shared.Models;

namespace BonusSystem.Shared.Dtos;

public record FindCompanyDto
{
    public string BIN { get; init; } = string.Empty;
    public string Number_of_contract { get; init; } = string.Empty;
    public string INN { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
public record CompanyDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public int Region { get; init; }
    public int City { get; init; }
    public string BIN { get; init; } = string.Empty;
    public string INN { get; init; } = string.Empty;
    public string Number_of_contract { get; set; } = string.Empty;
    public DateTime Date_of_contract { get; set; }
    public decimal BonusBalance { get; init; }
    public decimal FiatBalance { get; init; }
    public decimal InitialBonusBalance { get; init; }
    public decimal OriginalBonusBalance { get; init; }
    public CompanyStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<StoreDto> Stores { get; init; } = new();
}

public record CompanyRegistrationDto
{
    public string Name { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public int City { get; init; }
    public int Region { get; init; }
    public string BIK { get; init; } = string.Empty;
    public string BIN { get; set; } = string.Empty;
    public string INN { get; set; } = string.Empty;
    public string Number_of_contract { get; set; } = string.Empty;
    public DateTime Date_of_contract { get; set; }
}

public record CompanyUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public int City { get; init; }
    public int Region { get; init; }
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public string INN { get; set; } = string.Empty;
    public string BIK { get; set; } = string.Empty;
    public string BIN { get; set; } = string.Empty;
    public string Number_of_contract { get; set; } = string.Empty;
    public DateTime Date_of_contract { get; set; }
}

public record AppointAdminDto
{
    public Guid CompanyId { get; init; }
    public Guid UserId { get; init; }
    
}
public record CompanyRegistrationResultDto
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public CompanyDto? Company { get; init; }
    public string Token { get; set; }
}
public record StoreDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; } = string.Empty;
    public Guid CompanyId { get; init; }
    public int BusinessTypeId { get; init; }
    public int CategoryId { get; init; }
    public string PasswordHash { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string MallId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int City { get; init; } 
    public int Region { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Floor { get; init; } = string.Empty;
    public string Row { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string WorkingHours { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public string TypeOfbusiness { get; init; } = string.Empty;
    public StoreStatus Status { get; init; }
}

public class StoreRegistrationDto
{
    public Guid CompanyId { get; init; }
    public int BusinessTypeId { get; init; }
    public int CategoryId { get; init; }
    public string MallId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int City { get; init; }
    public int Region { get; init; } 
    public string Floor { get; init; } = string.Empty;
    public string Row { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string WorkingHours { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public List<CompanySellerDto> Sellers { get; init; }
}
public class StoreUpdateDto
{
    public string Email { get; init; } = string.Empty;
    public string Mall { get; init; } = string.Empty;
    public string Floor { get; init; } = string.Empty;
    public string Row { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int City { get; init; }
    public int Region { get; init; } 
    public string Address { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty; 
    public string WorkingHours { get; init; } = string.Empty;

}
public record CompanySummaryDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal BonusBalance { get; init; }
    public decimal TransactionVolume { get; init; }
    public int StoreCount { get; init; } 
    public string BIK { get; init; } = string.Empty;
    public CompanyStatus Status { get; init; }
}

public record CompanyRealTimeStatisticsDto
{
    public Guid CompanyId { get; init; }
    public DateTime Date { get; init; }
    public decimal BonusBalance { get; init; }
    public decimal WalletBalance { get; init; }
    public decimal SalesAmount { get; init; }
    public int DealCount { get; init; }
    public int RefundCount { get; init; }
    public decimal CashbackAmount { get; init; }
    public decimal Commission { get; init; }
    public string CommissionPaymentStatus { get; init; } = string.Empty;
    public DateTime LastUpdated { get; init; }
}

public record CompanyDailyStatisticsDto
{
    public Guid CompanyId { get; init; }
    public DateTime Date { get; init; }
    public decimal BonusBalance { get; init; }
    public decimal WalletBalance { get; init; }
    public decimal SalesAmount { get; init; }
    public int DealCount { get; init; }
    public int RefundCount { get; init; }
    public decimal CashbackAmount { get; init; }
    public decimal Commission { get; init; }
    public string CommissionPaymentStatus { get; init; } = string.Empty;
    public List<StoreDailyStatisticsDto> StoreStatistics { get; init; } = new();
}

public record StoreDailyStatisticsDto
{
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public decimal SalesAmount { get; init; }
    public int DealCount { get; init; }
    public int RefundCount { get; init; }
    public decimal Commission { get; init; }
}

public record CompanyQuarterlyStatisticsDto
{
    public Guid CompanyId { get; init; }
    public int Year { get; init; }
    public int Quarter { get; init; }
    public decimal TotalBonusBalance { get; init; }
    public decimal TotalWalletBalance { get; init; }
    public decimal TotalSalesAmount { get; init; }
    public int TotalDealCount { get; init; }
    public int TotalRefundCount { get; init; }
    public decimal TotalCashbackAmount { get; init; }
    public decimal TotalCommission { get; init; }
    public string CommissionPaymentStatus { get; init; } = string.Empty;
    public List<CompanyDailyStatisticsDto> DailyStatistics { get; init; } = new();
}

public record RealTimeStatisticsUpdateDto
{
    public Guid CompanyId { get; init; }
    public CompanyRealTimeStatisticsDto Statistics { get; init; }
    public string UpdateType { get; init; } = string.Empty; // "daily", "quarterly", "realtime"
    public DateTime Timestamp { get; init; }
}
using BonusSystem.Shared.Models;

namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class FiatTransactionEntity
{
    public Guid Id { get; set; }
    public string FrontendId { get; init; }
    public Guid? UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? SellerId { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal FiatCashBackRate { get; set; }
    public decimal FiatTransactionAmount { get; set; }
    public decimal FiatCashBackAmount { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime Timestamp { get; set; }
    public FiatTransactionStatus Status { get; set; }
    public string? Description { get; set; }
    public decimal CommissionPercent { get; set; }
    public Guid? RelatedBonusTransactionId { get; set; }

    // Navigation properties
    public UserEntity? User { get; set; }
    public CompanyEntity? Company { get; set; }
    public StoreEntity? Store { get; set; }
    public BonusTransactionEntity? RelatedBonusTransaction { get; set; }
}

public class FiatReplenishmentCompanyBalanceEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }

    // Navigation properties
    public CompanyEntity? Company { get; set; }
}
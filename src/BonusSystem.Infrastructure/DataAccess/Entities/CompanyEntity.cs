using BonusSystem.Shared.Models;

namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class CompanyEntity: BaseEntity
{
    public required string Name { get; set; }
    public required string ContactEmail { get; set; } 
    public required string PasswordHash { get; set; }
    public required string UserName { get; set; } 

    public required string BIN { get; set; } = string.Empty;
    public required string INN { get; set; } = string.Empty;
    public required string Number_of_contract { get; set; } = string.Empty;
    public required DateTime Date_of_contract { get; set; }
    
    public string? ContactPhone { get; set; }
    public int Region { get; set; }
    public int City { get; set; }
    public decimal BonusBalance { get; set; }
    public decimal FiatBalance { get; set; }
    public decimal OriginalBonusBalance { get; set; }
    public decimal InitialBonusBalance { get; set; } 
    public CompanyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<StoreEntity> Stores { get; set; } = new List<StoreEntity>();
    public ICollection<BonusTransactionEntity> Transactions { get; set; } = new List<BonusTransactionEntity>();
    public ICollection<FiatTransactionEntity> FiatTransactions { get; set; } = new List<FiatTransactionEntity>(); 
    public ICollection<FiatReplenishmentCompanyBalanceEntity> FiatReplenishmentCompanyBalances { get; set; } = new List<FiatReplenishmentCompanyBalanceEntity>();
    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    
}


using BonusSystem.Shared.Models;

namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class StoreEntity: BaseEntity
{
    public Guid CompanyId { get; set; }
    public int BusinessTypeId { get; set; }
    public int CategoryId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string MallId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int City { get; set; } 
    public int Region { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string WorkingHours { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string TypeOfbusiness { get; set; } = string.Empty;
    public StoreStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    

    // Navigation properties
    public CompanyEntity Company { get; set; } = null!;
    public ICollection<UserEntity> Sellers { get; set; } = new List<UserEntity>();
    public ICollection<BonusTransactionEntity> Transactions { get; set; } = new List<BonusTransactionEntity>();
    public ICollection<FiatTransactionEntity> FiatTransactions { get; set; } = new List<FiatTransactionEntity>();
}

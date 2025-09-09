using BonusSystem.Shared.Models;

namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class UserEntity: BaseEntity
{
    public required string Username { get; set; } 
    public required string Phone { get; set; } 
    public required string FirstName { get; set; } 
    public required string LastName { get; set; } 
    public required int City { get; set; } 
    public required int Region { get; set; } 
    public required string? INN { get; set; } 
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; }
    public decimal BonusBalance { get; set; }
    public decimal FiatBalance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Company relationship
    public Guid? CompanyId { get; set; }

    public Guid? StoreId { get; set; }
    public CompanyEntity? Company { get; set; }

    // Navigation properties
    public StoreEntity? Store { get; set; }

    public ICollection<BonusTransactionEntity> Transactions { get; set; } = new List<BonusTransactionEntity>();

    public ICollection<FiatTransactionEntity> FiatTransactions { get; set; } = new List<FiatTransactionEntity>();

    public ICollection<NotificationEntity> Notifications { get; set; } = new List<NotificationEntity>();

    // Email verification
    public string? VerificationCode { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool PincodeSet { get; set; }
    public string? DeviceToken { get; set; }
}

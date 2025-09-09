namespace BonusSystem.Shared.Models;

public enum UserRole
{
    Buyer,
    Seller,
    StoreAdmin,
    SystemAdmin,
    CompanyObserver,
    SystemObserver,
    Company
}
public enum Categories
{
    Shopping,
    Beauty,
    Health,
    Food
}
public enum CompanyStatus
{
    Active,
    Suspended,
    Pending
}
public enum TransactionStatusConfirm
{
    Pending = 0,
    Completed = 1,
}
public enum StoreStatus
{
    Active,
    Inactive,
    PendingApproval
}

public enum TransactionType
{
    Earn,
    Spend,
    Expire,
    AdminAdjustment,
    Transfer,
    Replenishment
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Reversed,
    Failed
}

public enum FiatTransactionStatus
{
    Pending,
    Confirmed, // Добавлен статус подтверждения
    Completed,
    Reversed,
    Failed
}
public enum TransactionReturnStatus
{
    Pending,
    Approved,
    Rejected
}
public enum NotificationType
{
    Transaction,
    System,
    Expiration,
    AdminMessage
}
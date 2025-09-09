using BonusSystem.Shared.Models; 

namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class TransferEntity
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal? CommissionPercent { get; set; }
    public DateTime Timestamp { get; set; } 

    public UserEntity? Sender { get; set; }
    public UserEntity? Recipient { get; set; }
}
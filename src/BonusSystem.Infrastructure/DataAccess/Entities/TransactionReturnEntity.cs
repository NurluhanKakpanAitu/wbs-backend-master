using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Models;

public class TransactionReturn
{
    public Guid Id { get; set; }
    public Guid? BonusTransactionId { get; set; }
    public Guid? FiatTransactionId { get; set; }
    public string Reason { get; set; }
    public TransactionReturnStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }

    // Navigation properties
    public UserEntity RequestedByUser { get; set; }
    public UserEntity? ApprovedByUser { get; set; }
    public BonusTransactionEntity? BonusTransaction { get; set; }
    public FiatTransactionEntity? FiatTransaction { get; set; }
}
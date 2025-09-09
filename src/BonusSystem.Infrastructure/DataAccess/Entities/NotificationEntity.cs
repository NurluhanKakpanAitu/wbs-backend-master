using BonusSystem.Shared.Models;

namespace BonusSystem.Infrastructure.DataAccess.Entities;

public class NotificationEntity
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public required string Message { get; set; }
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    
    // Navigation properties
    public UserEntity Recipient { get; set; } = null!;
}

using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;

namespace BonusSystem.Core.Repositories;

/// <summary>
/// Repository for notification-related operations
/// </summary>
public interface INotificationRepository
{
    Task<bool> SendNotificationAsync(Guid userId, string message, NotificationType type);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId);
    Task<bool> MarkNotificationAsReadAsync(Guid notificationId);
    Task<bool> SendBulkNotificationsAsync(IEnumerable<Guid> userIds, string message, NotificationType type);
    Task<bool> SendNotificationToRoleAsync(UserRole role, string message, NotificationType type);
}
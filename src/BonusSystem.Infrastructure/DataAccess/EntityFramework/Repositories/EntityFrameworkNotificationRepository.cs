using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BonusSystem.Shared.Dtos; 
namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkNotificationRepository : INotificationRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkNotificationRepository> _logger;

    public EntityFrameworkNotificationRepository(BonusSystemContext dbContext,
        ILogger<EntityFrameworkNotificationRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> SendNotificationAsync(Guid userId, string message, NotificationType type)
    {
        try
        {
            var notification = new NotificationEntity
            {
                Id = Guid.NewGuid(),
                RecipientId = userId,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetUserNotificationsMessagesAsync(Guid userId)
    {
        try
        {
            var notifications = await _dbContext.Notifications.AsNoTracking()
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => n.Message)
                .ToListAsync();

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId)
    {
        try
        {
            var notifications = await _dbContext.Notifications.AsNoTracking()
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Message = n.Message,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    RecipientId = n.RecipientId
                })
                .ToListAsync();

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
    {
        try
        {
            var notification = await _dbContext.Notifications.FindAsync(notificationId);
            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            throw;
        }
    }

    public async Task<bool> SendBulkNotificationsAsync(IEnumerable<Guid> userIds, string message, NotificationType type)
    {
        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var notifications = userIds.Select(userId => new NotificationEntity
                    {
                        Id = Guid.NewGuid(),
                        RecipientId = userId,
                        Message = message,
                        Type = type,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });

                    await _dbContext.Notifications.AddRangeAsync(notifications);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notifications to {UserCount} users", userIds.Count());
            throw;
        }
    }

    public async Task<bool> SendNotificationToRoleAsync(UserRole role, string message, NotificationType type)
    {
        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Find all users with the specified role
                    var userIds = await _dbContext.Users.AsNoTracking()
                        .Where(u => u.Role == role)
                        .Select(u => u.Id)
                        .ToListAsync();

                    // Create notifications for each user
                    var notifications = userIds.Select(userId => new NotificationEntity
                    {
                        Id = Guid.NewGuid(),
                        RecipientId = userId,
                        Message = message,
                        Type = type,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    });

                    await _dbContext.Notifications.AddRangeAsync(notifications);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notifications to users with role {Role}", role);
            throw;
        }
    }
}
using BonusSystem.Shared.Dtos;

namespace BonusSystem.Core.Services.Interfaces;

/// <summary>
/// Service for sending Firebase push notifications
/// </summary>
public interface IFirebasePushNotificationService
{
    /// <summary>
    /// Send push notification to a single user
    /// </summary>
    Task<PushNotificationResponse> SendToUserAsync(PushNotificationToUserRequest request);
    
    /// <summary>
    /// Send push notification to multiple users
    /// </summary>
    Task<PushNotificationResponse> SendToMultipleUsersAsync(PushNotificationToMultipleUsersRequest request);
    
    /// <summary>
    /// Send push notification to all users with a specific role
    /// </summary>
    Task<PushNotificationResponse> SendToRoleAsync(PushNotificationToRoleRequest request);
    
    /// <summary>
    /// Update user's device token
    /// </summary>
    Task<bool> UpdateDeviceTokenAsync(Guid userId, string deviceToken);
    
    /// <summary>
    /// Remove user's device token
    /// </summary>
    Task<bool> RemoveDeviceTokenAsync(Guid userId);
    
    /// <summary>
    /// Send push notification with custom data
    /// </summary>
    Task<PushNotificationResponse> SendWithDataAsync(PushNotificationRequest request, Dictionary<string, string> data);
    
    /// <summary>
    /// Test push notification to a specific device token (for testing purposes)
    /// </summary>
    Task<PushNotificationResponse> SendTestNotificationAsync(string deviceToken, string title, string body);

    /// <summary>
    /// Send data-only message (silent notification) to a specific device token
    /// </summary>
    Task<PushNotificationResponse> SendDataOnlyMessageAsync(string deviceToken, Dictionary<string, string> data);

    /// <summary>
    /// Get user's device token from the database
    /// </summary>
    Task<string?> GetUserDeviceTokenAsync(Guid userId);
}

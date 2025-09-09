using BonusSystem.Shared.Models;

namespace BonusSystem.Shared.Dtos;

public record PushNotificationRequest
{
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public Dictionary<string, string>? Data { get; init; }
    public NotificationType Type { get; init; } = NotificationType.System;
}

public record PushNotificationToUserRequest : PushNotificationRequest
{
    public Guid UserId { get; init; }
}

public record PushNotificationToMultipleUsersRequest : PushNotificationRequest
{
    public IEnumerable<Guid> UserIds { get; init; } = new List<Guid>();
}

public record PushNotificationToRoleRequest : PushNotificationRequest
{
    public UserRole Role { get; init; }
}

public record PushNotificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public record DeviceTokenUpdateRequest
{
    public Guid UserId { get; init; }
    public string DeviceToken { get; init; } = string.Empty;
}

public record DeviceTokenUpdateRequestWithoutUserId
{
    public string DeviceToken { get; init; } = string.Empty;
}

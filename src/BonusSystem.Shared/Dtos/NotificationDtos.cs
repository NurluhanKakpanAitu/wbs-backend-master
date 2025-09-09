using BonusSystem.Shared.Models;

namespace BonusSystem.Shared.Dtos;

public record struct NotificationRequest()
{
    public string Message { get; init; } = string.Empty;
    public NotificationType Type { get; init; } = NotificationType.System;
} 

public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid RecipientId { get; init; }
    public string Message { get; init; } = string.Empty;
    public NotificationType Type { get; init; } = NotificationType.System;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsRead { get; init; } = false;
}
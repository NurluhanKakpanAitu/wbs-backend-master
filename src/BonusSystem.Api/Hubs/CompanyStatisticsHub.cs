using Microsoft.AspNetCore.SignalR;
using BonusSystem.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace BonusSystem.Api.Hubs;

[Authorize]
public class CompanyStatisticsHub : Hub
{
    private readonly ILogger<CompanyStatisticsHub> _logger;
    private static readonly Dictionary<string, string> _userCompanyMap = new();

    public CompanyStatisticsHub(ILogger<CompanyStatisticsHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinCompanyGroup(string companyId)
    {
        var groupName = $"Company_{companyId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _userCompanyMap[Context.ConnectionId] = companyId;
        
        _logger.LogInformation("User {UserId} joined company group {CompanyId}", Context.UserIdentifier, companyId);
    }

    public async Task LeaveCompanyGroup(string companyId)
    {
        var groupName = $"Company_{companyId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _userCompanyMap.Remove(Context.ConnectionId);
        
        _logger.LogInformation("User {UserId} left company group {CompanyId}", Context.UserIdentifier, companyId);
    }

    public async Task SubscribeToRealTimeUpdates(string companyId)
    {
        var groupName = $"Company_{companyId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} subscribed to real-time updates for company {CompanyId}", 
            Context.UserIdentifier, companyId);
    }

    public async Task UnsubscribeFromRealTimeUpdates(string companyId)
    {
        var groupName = $"Company_{companyId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} unsubscribed from real-time updates for company {CompanyId}", 
            Context.UserIdentifier, companyId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_userCompanyMap.TryGetValue(Context.ConnectionId, out var companyId))
        {
            _userCompanyMap.Remove(Context.ConnectionId);
            _logger.LogInformation("User {UserId} disconnected from company {CompanyId}", 
                Context.UserIdentifier, companyId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Static method to broadcast updates to all connected clients for a specific company
    public static async Task BroadcastStatisticsUpdate(IHubContext<CompanyStatisticsHub> hubContext, 
        RealTimeStatisticsUpdateDto update)
    {
        var groupName = $"Company_{update.CompanyId}";
        await hubContext.Clients.Group(groupName).SendAsync("StatisticsUpdated", update);
    }

    // Static method to broadcast daily statistics
    public static async Task BroadcastDailyStatistics(IHubContext<CompanyStatisticsHub> hubContext, 
        CompanyDailyStatisticsDto dailyStats)
    {
        var groupName = $"Company_{dailyStats.CompanyId}";
        await hubContext.Clients.Group(groupName).SendAsync("DailyStatisticsUpdated", dailyStats);
    }

    // Static method to broadcast quarterly statistics
    public static async Task BroadcastQuarterlyStatistics(IHubContext<CompanyStatisticsHub> hubContext, 
        CompanyQuarterlyStatisticsDto quarterlyStats)
    {
        var groupName = $"Company_{quarterlyStats.CompanyId}";
        await hubContext.Clients.Group(groupName).SendAsync("QuarterlyStatisticsUpdated", quarterlyStats);
    }
}

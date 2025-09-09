# Real-Time Monitoring System for Company Statistics

## Overview

This system provides real-time monitoring of company statistics including:
- Bonus balance
- Wallet balance
- Sales amount
- Deal count
- Refund count
- Cashback amount
- Commission
- Commission payment status

## Features

### 1. Real-Time Statistics
- **Current Statistics**: Get up-to-date statistics for any company
- **Daily Statistics**: Detailed breakdown by day with store-level data
- **Monthly Statistics**: Aggregated monthly data
- **Quarterly Statistics**: Complete quarterly overview with daily breakdowns

### 2. Real-Time Updates
- **SignalR Hub**: WebSocket-based real-time communication
- **Automatic Updates**: Statistics update every minute when monitoring is active
- **Live Notifications**: Instant updates to connected clients

### 3. Monitoring Control
- **Start Monitoring**: Begin real-time tracking for a company
- **Stop Monitoring**: Halt real-time updates
- **Status Check**: Verify if monitoring is active

## API Endpoints

### Real-Time Statistics
```
GET /api/company/{companyId}/statistics/realtime
GET /api/company/{companyId}/statistics/daily/{date}
GET /api/company/{companyId}/statistics/monthly/{year}/{month}
GET /api/company/{companyId}/statistics/quarterly/{year}/{quarter}
GET /api/company/{companyId}/statistics/at-date/{date}
```

### Monitoring Control
```
POST /api/company/{companyId}/monitoring/start
POST /api/company/{companyId}/monitoring/stop
GET /api/company/{companyId}/monitoring/status
```

### SignalR Hub
```
/hubs/company-statistics
```

## SignalR Client Implementation

### JavaScript/TypeScript Example

```typescript
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

class CompanyStatisticsClient {
    private connection: HubConnection;
    private companyId: string;

    constructor(companyId: string) {
        this.companyId = companyId;
        this.connection = new HubConnectionBuilder()
            .withUrl('/hubs/company-statistics')
            .withAutomaticReconnect()
            .build();
    }

    async connect() {
        try {
            await this.connection.start();
            console.log('Connected to SignalR Hub');
            
            // Join company group
            await this.connection.invoke('JoinCompanyGroup', this.companyId);
            
            // Subscribe to real-time updates
            await this.connection.invoke('SubscribeToRealTimeUpdates', this.companyId);
            
            // Set up event handlers
            this.setupEventHandlers();
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
        }
    }

    private setupEventHandlers() {
        // Real-time statistics updates
        this.connection.on('StatisticsUpdated', (update) => {
            console.log('Statistics Updated:', update);
            this.handleStatisticsUpdate(update);
        });

        // Daily statistics updates
        this.connection.on('DailyStatisticsUpdated', (dailyStats) => {
            console.log('Daily Statistics Updated:', dailyStats);
            this.handleDailyStatisticsUpdate(dailyStats);
        });

        // Quarterly statistics updates
        this.connection.on('QuarterlyStatisticsUpdated', (quarterlyStats) => {
            console.log('Quarterly Statistics Updated:', quarterlyStats);
            this.handleQuarterlyStatisticsUpdate(quarterlyStats);
        });
    }

    private handleStatisticsUpdate(update: any) {
        // Update UI with real-time data
        this.updateDashboard(update.statistics);
    }

    private handleDailyStatisticsUpdate(dailyStats: any) {
        // Update daily statistics view
        this.updateDailyView(dailyStats);
    }

    private handleQuarterlyStatisticsUpdate(quarterlyStats: any) {
        // Update quarterly statistics view
        this.updateQuarterlyView(quarterlyStats);
    }

    private updateDashboard(statistics: any) {
        // Update dashboard elements
        document.getElementById('bonus-balance')!.textContent = statistics.bonusBalance;
        document.getElementById('wallet-balance')!.textContent = statistics.walletBalance;
        document.getElementById('sales-amount')!.textContent = statistics.salesAmount;
        document.getElementById('deal-count')!.textContent = statistics.dealCount;
        document.getElementById('refund-count')!.textContent = statistics.refundCount;
        document.getElementById('cashback-amount')!.textContent = statistics.cashbackAmount;
        document.getElementById('commission')!.textContent = statistics.commission;
        document.getElementById('commission-status')!.textContent = statistics.commissionPaymentStatus;
    }

    async disconnect() {
        if (this.connection) {
            await this.connection.stop();
        }
    }
}

// Usage
const client = new CompanyStatisticsClient('your-company-id');
client.connect();
```

### C# Client Example

```csharp
using Microsoft.AspNetCore.SignalR.Client;

public class CompanyStatisticsClient
{
    private HubConnection _hubConnection;
    private readonly string _companyId;

    public CompanyStatisticsClient(string companyId)
    {
        _companyId = companyId;
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://your-api-url/hubs/company-statistics")
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine("Connected to SignalR Hub");

            // Join company group
            await _hubConnection.InvokeAsync("JoinCompanyGroup", _companyId);

            // Subscribe to real-time updates
            await _hubConnection.InvokeAsync("SubscribeToRealTimeUpdates", _companyId);

            // Set up event handlers
            SetupEventHandlers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR Connection Error: {ex.Message}");
        }
    }

    private void SetupEventHandlers()
    {
        _hubConnection.On<RealTimeStatisticsUpdateDto>("StatisticsUpdated", (update) =>
        {
            Console.WriteLine($"Statistics Updated: {update}");
            HandleStatisticsUpdate(update);
        });

        _hubConnection.On<CompanyDailyStatisticsDto>("DailyStatisticsUpdated", (dailyStats) =>
        {
            Console.WriteLine($"Daily Statistics Updated: {dailyStats}");
            HandleDailyStatisticsUpdate(dailyStats);
        });

        _hubConnection.On<CompanyQuarterlyStatisticsDto>("QuarterlyStatisticsUpdated", (quarterlyStats) =>
        {
            Console.WriteLine($"Quarterly Statistics Updated: {quarterlyStats}");
            HandleQuarterlyStatisticsUpdate(quarterlyStats);
        });
    }

    private void HandleStatisticsUpdate(RealTimeStatisticsUpdateDto update)
    {
        // Update UI or process data
        Console.WriteLine($"Real-time update for company {update.CompanyId}: Sales: {update.Statistics.SalesAmount}");
    }

    private void HandleDailyStatisticsUpdate(CompanyDailyStatisticsDto dailyStats)
    {
        // Process daily statistics
        Console.WriteLine($"Daily stats for {dailyStats.Date}: {dailyStats.DealCount} deals");
    }

    private void HandleQuarterlyStatisticsUpdate(CompanyQuarterlyStatisticsDto quarterlyStats)
    {
        // Process quarterly statistics
        Console.WriteLine($"Q{quarterlyStats.Quarter} {quarterlyStats.Year}: Total sales: {quarterlyStats.TotalSalesAmount}");
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
        }
    }
}
```

## Data Models

### CompanyRealTimeStatisticsDto
```csharp
public record CompanyRealTimeStatisticsDto
{
    public Guid CompanyId { get; init; }
    public DateTime Date { get; init; }
    public decimal BonusBalance { get; init; }
    public decimal WalletBalance { get; init; }
    public decimal SalesAmount { get; init; }
    public int DealCount { get; init; }
    public int RefundCount { get; init; }
    public decimal CashbackAmount { get; init; }
    public decimal Commission { get; init; }
    public string CommissionPaymentStatus { get; init; }
    public DateTime LastUpdated { get; init; }
}
```

### CompanyDailyStatisticsDto
```csharp
public record CompanyDailyStatisticsDto
{
    public Guid CompanyId { get; init; }
    public DateTime Date { get; init; }
    public decimal BonusBalance { get; init; }
    public decimal WalletBalance { get; init; }
    public decimal SalesAmount { get; init; }
    public int DealCount { get; init; }
    public int RefundCount { get; init; }
    public decimal CashbackAmount { get; init; }
    public decimal Commission { get; init; }
    public string CommissionPaymentStatus { get; init; }
    public List<StoreDailyStatisticsDto> StoreStatistics { get; init; }
}
```

## Configuration

### 1. Service Registration
The system automatically registers all required services in `ApiExtensions.cs`:
- `IRealTimeMonitoringService`
- SignalR services

### 2. SignalR Hub Mapping
The hub is automatically mapped in `Program.cs`:
```csharp
app.MapHub<CompanyStatisticsHub>("/hubs/company-statistics");
```

### 3. CORS Configuration
CORS is configured to allow SignalR connections from specified origins.

## Performance Considerations

### 1. Caching
- Statistics are cached for 5 minutes to reduce database queries
- Real-time updates are sent only when data changes

### 2. Monitoring Sessions
- Active monitoring sessions are tracked in memory
- Timers automatically clean up inactive sessions

### 3. Database Optimization
- Queries are optimized with proper indexing
- Date-based filtering reduces data transfer

## Security

### 1. Authentication
- All endpoints require JWT authentication
- SignalR connections are authenticated

### 2. Authorization
- Users can only access statistics for their own company
- Company ID validation prevents unauthorized access

### 3. Rate Limiting
- Consider implementing rate limiting for high-frequency requests
- Monitor connection limits for SignalR

## Monitoring and Logging

### 1. Logging
- All operations are logged with appropriate levels
- Connection events are tracked for debugging

### 2. Error Handling
- Graceful error handling for connection failures
- Automatic reconnection for SignalR clients

### 3. Performance Metrics
- Monitor response times for statistics queries
- Track SignalR connection counts

## Troubleshooting

### Common Issues

1. **SignalR Connection Fails**
   - Check CORS configuration
   - Verify authentication token
   - Check network connectivity

2. **Statistics Not Updating**
   - Verify monitoring is active
   - Check database connectivity
   - Review error logs

3. **High Memory Usage**
   - Monitor active monitoring sessions
   - Check for memory leaks in timers
   - Review caching strategy

### Debug Mode
Enable detailed logging in development:
```csharp
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
}
```

## Future Enhancements

1. **WebSocket Fallback**: Implement WebSocket fallback for SignalR
2. **Redis Backplane**: Scale SignalR across multiple instances
3. **Advanced Caching**: Implement Redis-based caching
4. **Metrics Dashboard**: Built-in monitoring dashboard
5. **Alert System**: Configure alerts for specific thresholds
6. **Export Functionality**: Export statistics to various formats
7. **Historical Analysis**: Long-term trend analysis
8. **Mobile Notifications**: Push notifications for mobile apps

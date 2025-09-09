using BonusSystem.Api.Infrastructure.Swagger;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Models;
using Microsoft.OpenApi.Models;

namespace BonusSystem.Api.Features.Observers;

public static class ObserverEndpoints
{
    public static WebApplication MapObserverEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/observers")
            .RequireAuthorization()
            .WithTags("Observers")
            .WithOpenApi();

        group.MapGet("/context", ObserverHandlers.GetUserContext)
            .WithName("GetObserverContext")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Observer Context";
                operation.Description = "Retrieves the observer's user context and permitted actions. Observers have read-only access to system data for analysis and reporting purposes.\n\n" +
                    "Successful response contains:\n" +
                    "- context: Observer's user context information (ID, username, role)\n" +
                    "- actions: List of actions permitted for the observer";
                
                operation.EnsureResponse("200", "Returns observer context and actions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/statistics", ObserverHandlers.GetStatistics)
            .WithName("GetObserverStatistics")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get System Statistics";
                operation.Description = "Retrieves comprehensive system-wide or company-specific statistics with optional date filtering. Provides aggregated metrics for analysis of the bonus program's performance.\n\n" +
                    "Query parameters:\n" +
                    "- companyId: Optional. Filter statistics for a specific company\n" +
                    "- startDate: Optional. Start date for the statistics period\n" +
                    "- endDate: Optional. End date for the statistics period\n\n" +
                    "Successful response contains aggregated metrics such as transaction count, active users count, total bonuses earned/spent/expired, etc.";
                
                operation.EnsureResponse("200", "Returns system statistics");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/transactions/summary", ObserverHandlers.GetTransactionSummary)
            .WithName("GetObserverTransactionSummary")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Transaction Summary";
                operation.Description = "Retrieves a summary of all transactions in the system or for a specific company. Provides aggregated transaction metrics categorized by type, status, and other dimensions.\n\n" +
                    "Query parameters:\n" +
                    "- companyId: Optional. Company ID to get transaction summary for. If not provided, returns system-wide summary\n\n" +
                    "Successful response contains transaction metrics grouped by type, status, volume, etc.";
                
                operation.EnsureResponse("200", "Returns transaction summary");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/companies", ObserverHandlers.GetCompaniesOverview)
            .WithName("GetObserverCompaniesOverview")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Companies Overview";
                operation.Description = "Retrieves an overview of all companies in the system with their basic information and statistics. Provides a comparative view of company participation in the bonus program.\n\n" +
                    "Successful response contains a list of companies with summary data for each:\n" +
                    "- id: Company identifier\n" +
                    "- name: Company name\n" +
                    "- bonusBalance: Current bonus balance\n" +
                    "- transactionVolume: Total volume of transactions\n" +
                    "- storeCount: Number of stores\n" +
                    "- status: Company status (0=Active, 1=Suspended, 2=Pending)";
                
                operation.EnsureResponse("200", "Returns companies overview");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
            
        return app;
    }
}

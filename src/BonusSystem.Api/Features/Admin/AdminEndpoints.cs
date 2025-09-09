using System.Numerics;
using BonusSystem.Api.Infrastructure.Swagger;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.OpenApi.Models;

namespace BonusSystem.Api.Features.Admin;

public static class AdminEndpoints
{
    public static WebApplication MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin")
            .RequireAuthorization()
            .WithTags("Admin")
            .WithOpenApi();

        group.MapGet("/context", AdminHandlers.GetUserContext)
            .WithName("GetAdminContext")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Admin Context";
                operation.Description = "Retrieves the administrator's user context and permitted actions. This is typically called after login to initialize the admin dashboard.\n\n" +
                    "Successful response contains:\n" +
                    "- context: Admin's user context information (ID, username, role)\n" +
                    "- actions: List of actions permitted for the admin";

                operation.EnsureResponse("200", "Returns admin context and actions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapPost("/register_company", AdminHandlers.RegisterCompany)
            .WithName("RegisterCompany")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a New Company (Legacy)";
                operation.Description = "Creates a new company record in the system. This is a legacy endpoint and is recommended to use the new endpoint (/api/admin/companies-with-admin) instead.\n\n" +
                    "Request requires:\n" +
                    "- name: Name of the company\n" +
                    "- contactEmail: Primary contact email for the company\n" +
                    "- contactPhone: Primary contact phone number\n" +
                    "- initialBonusBalance: Initial bonus points to credit to the company\n\n" +
                    "Successful response contains the registered company details.";

                operation.EnsureResponse("200", "Company registered successfully");
                operation.EnsureResponse("400", "Registration failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });


        // group.MapPost("/appoint-admin", AdminHandlers.AppointAdmin)
        //     .WithName("AppointAdmin")
        //     .RequireAuthorization()
        //     .WithOpenApi(operation =>
        //     {

        //         operation.Summary = "Appoint a New Admin User";
        //         operation.Description = "Assigns a new admin user to a company. This is a legacy endpoint and is recommended to use the new endpoint (/api/admin/companies-with-admin) instead.\n\n" +
        //             "Request requires:\n" +
        //             "- companyId: ID of the company to which the admin will be appointed\n" +
        //             "- adminUsername: Username for the company admin\n" +
        //             "- adminEmail: Email for the company admin\n" +
        //             "- adminPassword: Password for the company admin\n\n" +
        //             "Successful response contains a confirmation message.";
        //         return operation;
        //     });

        group.MapPut("/companies/{id}/status", AdminHandlers.UpdateCompanyStatus)
            .WithName("UpdateCompanyStatus")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update Company Status";
                operation.Description = "Updates the status of a company, controlling whether it can actively participate in the bonus system. Useful for temporarily suspending a company or reactivating it.\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the company\n\n" +
                    "Request body is the new status value:\n" +
                    "- 0 = Active: Company can participate in bonus system\n" +
                    "- 1 = Suspended: Company is temporarily disabled\n" +
                    "- 2 = Pending: Company is pending approval\n\n" +
                    "Successful response contains a confirmation message.";

                operation.EnsureResponse("200", "Company status updated");
                operation.EnsureResponse("400", "Status update failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });

        group.MapPut("/stores/{id}/moderate", AdminHandlers.ModerateStore)
            .WithName("ModerateStore")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Moderate a Store";
                operation.Description = "Approves or rejects a store registration that is pending approval. Companies must register their stores, but these registrations require administrator approval before becoming active.\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the store to moderate\n\n" +
                    "Query parameters:\n" +
                    "- approve: Set to true to approve the store, false to reject\n\n" +
                    "Successful response contains a confirmation message.";

                operation.EnsureResponse("200", "Store moderation completed");
                operation.EnsureResponse("400", "Moderation failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapPost("/companies/{id}/credit-fiat", AdminHandlers.ReplenishmentFiatCompany)
            .WithName("CreditCompanyFiatBalance")
            .RequireAuthorization()
            .WithOpenApi(
                operation =>
                {
                    operation.Summary = "Credit Company Fiat Balance";
                    operation.Description = "Credits the specified amount to the company's fiat balance.";

                    operation.EnsureResponse("200", "Company fiat balance credited successfully");
                    operation.EnsureResponse("400", "Invalid request");
                    operation.EnsureResponse("401", "Unauthorized");
                    operation.EnsureResponse("500", "Internal server error");

                    return operation;
                }
            );
        group.MapPost("/companies/{id}/credit", AdminHandlers.CreditCompanyBalance)
            .WithName("CreditCompanyBonusBalance")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Credit Company Balance";
                operation.Description = "Adds bonus balance to a company's account. This increases both the current balance and the original balance (baseline) that will persist through quarterly expirations.\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the company\n\n" +
                    "Request body contains the amount of bonus balance to credit to the company.\n\n" +
                    "Successful response contains a confirmation message.";

                operation.EnsureResponse("200", "Company balance credited");
                operation.EnsureResponse("400", "Credit failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });

        group.MapPost("/companies/transaction-fees", AdminHandlers.GetTransactionFeeReport)
            .WithName("GetTransactionFeeReport")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get companies transaction fees";
                operation.Description = "Allow to calculate the list of companies transaction fees based on all system COMPLETED transactions.\n\n"
                    + "Path parameters:\n"
                    + "- feePercent: transaction fee value\n\n"
                    + "- fromDate: (optional) filtering by start date\n\n"
                    + "- endDate: (optional) filtering by end date.";

                operation.EnsureResponse("200", "Fees calculated successfully");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Error at getting company fees");

                return operation;
            });

        group.MapGet("/transactions", AdminHandlers.GetSystemTransactions)
            .WithName("GetSystemTransactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get System Transactions";
                operation.Description = "Retrieves transaction records across the entire system with optional filtering by company, date range, and other criteria. Provides a comprehensive view for system administrators.\n\n" +
                    "Query parameters:\n" +
                    "- companyId: Optional. Filter transactions by company ID\n" +
                    "- startDate: Optional. Filter transactions with timestamp on or after this date\n" +
                    "- endDate: Optional. Filter transactions with timestamp on or before this date\n\n" +
                    "Successful response contains a list of transactions matching the filter criteria.";

                operation.EnsureResponse("200", "Returns system transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });

        group.MapPost("/notifications", AdminHandlers.SendSystemNotification)
            .WithName("SendSystemNotification")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Send System Notification";
                operation.Description = "Sends a system notification to a specific user or to all users. Used for communicating important system information, promotional offers, or administrative messages.\n\n" +
                    "Query parameters:\n" +
                    "- recipientId: Optional. Specific user ID to send notification to. If not provided, sends to all users\n\n" +
                    "Request requires:\n" +
                    "- message: Notification message content\n" +
                    "- type: Notification type (0=Transaction, 1=System, 2=Expiration, 3=AdminMessage)\n\n" +
                    "Successful response contains a confirmation message.";

                operation.EnsureResponse("200", "Notification sent");
                operation.EnsureResponse("400", "Sending failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapGet("/fiatTransactions", AdminHandlers.GetSystemFiatTransactions)
            .WithName("GetSystemFiatTransactions")
            .RequireAuthorization()
            .Produces<List<FiatTransactionDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get System Fiat Transactions";
                operation.Description = "Retrieves fiat transaction records across the entire system with optional filtering by company, date range, and other criteria. Provides a comprehensive view for system administrators.\n\n" +
                    "Query parameters:\n" +
                    "- companyId: Optional. Filter transactions by company ID\n" +
                    "- startDate: Optional. Filter transactions with timestamp on or after this date\n" +
                    "- endDate: Optional. Filter transactions with timestamp on or before this date\n\n" +
                    "Successful response contains a list of transactions matching the filter criteria.";

                operation.EnsureResponse("200", "Returns system transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapGet("/search-company", AdminHandlers.SearchCompany)
            .WithName("SearchCompany")
            .RequireAuthorization()
            .Produces<List<CompanySummaryDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search Company";
                operation.Description = "Searches for companies by name or BIN. Returns a list of companies matching the search criteria.\n\n" +
                    "Query parameters:\n" +
                    "- query: Search term (company name or BIN)\n\n" +
                    "Successful response contains a list of companies matching the search criteria.";

                operation.EnsureResponse("200", "Returns company search results");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapPost("/update-company", AdminHandlers.UpdateCompany)
            .WithName("UpdateCompany")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update Company";
                operation.Description = "Updates a company's information in the system.\n\n" +
                    "Request body contains the updated company information.\n\n" +
                    "Successful response contains the updated company information.";
                return operation;
            });
        group.MapPost("/send-company-url", AdminHandlers.SendCompanyUrl)
            .WithName("SendCompanyUrl")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Send Company URL";
                operation.Description = "Sends a URL to a specific company to initiate a payment request.\n\n" +
                    "Request body contains the URL to be sent.\n\n" +
                    "Successful response contains a confirmation message.";
                return operation;
            });
        return app; 
    }
}

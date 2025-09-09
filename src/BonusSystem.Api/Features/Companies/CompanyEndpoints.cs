using BonusSystem.Api.Infrastructure.Swagger;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.OpenApi.Models;
using BonusSystem.Api.Helpers;

namespace BonusSystem.Api.Features.Companies;

public static class CompanyEndpoints
{
    public static WebApplication MapCompanyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companies")
            .RequireAuthorization()
            .WithTags("Companies")
            .WithOpenApi();

        group.MapGet("/lastTransaction", CompanyHandlers.GetLastTransaction)
            .WithName("GetLastCompanyTransaction")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Last Company Transaction";
                operation.Description =
                    "Retrieves the most recent transaction for the company. This is useful for monitoring bonus point activity.\n\n" +
                    "Successful response contains the most recent transaction data or a placeholder if no transactions exist.";

                operation.EnsureResponse("200", "Returns last transaction data");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
            
        group.MapGet("/context", CompanyHandlers.GetUserContext)
            .WithName("GetCompanyContext")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Company Context";
                operation.Description =
                    "Retrieves the current company user's context information including profile data and permitted actions. This is typically called after login to initialize the company's dashboard.\n\n" +
                    "Successful response contains:\n" +
                    "- context: Company user's profile information (ID, username, role, bonus balance)\n" +
                    "- actions: List of permitted actions for the company";

                operation.EnsureResponse("200", "Returns company context and allowed actions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });

        group.MapPost("/stores", CompanyHandlers.RegisterStore)
            .WithName("RegisterStore")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a New Store";
                operation.Description =
                    "Creates a new store associated with the company. New stores are set to 'PendingApproval' status until approved by an administrator.\n\n" +
                    "Request requires:\n" +
                    "- companyId: ID of the company the store belongs to\n" +
                    "- name: Name of the store\n" +
                    "- location: Geographic location of the store\n" +
                    "- address: Physical address of the store\n" +
                    "- contactPhone: Contact phone number for the store\n" +
                    "- sellerIds: List of seller IDs to assign to this store (optional)\n\n" +
                    "Successful response returns a success confirmation.";

                operation.EnsureResponse("200", "Store registered successfully");
                operation.EnsureResponse("400", "Registration failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapGet("/get-stores", CompanyHandlers.GetStores)
            .WithName("GetStores")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Stores";
                operation.Description =
                    "Retrieves a list of stores associated with the company.\n\n" +
                    "Successful response contains a list of stores.";

                operation.EnsureResponse("200", "Returns list of stores");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapGet("/get-sellers", CompanyHandlers.GetSellers)
            .WithName("GetSellers")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Sellers";
                operation.Description =
                    "Retrieves a list of sellers associated with the company.\n\n" +
                    "Successful response contains a list of sellers.";
                return operation; 
            });

            group.MapDelete("/stores/{storeId}", CompanyHandlers.DeleteStore)
                .WithName("DeleteStore")
                .RequireAuthorization()
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Delete Store";
                    operation.Description = "Deletes a store by its ID. Only company or admin users can perform this operation.";
                    operation.EnsureResponse("200", "Store deleted successfully");
                    operation.EnsureResponse("400", "Deletion failed");
                    operation.EnsureResponse("401", "Unauthorized");
                    operation.EnsureResponse("500", "Internal server error");
                    return operation;
                });

            group.MapPut("/stores/{storeId}/address", CompanyHandlers.UpdateStoreAddress)
                .WithName("UpdateStoreAddress")
                .RequireAuthorization()
                .WithOpenApi(operation =>
                {
                    operation.Summary = "Update Store Address";
                    operation.Description = "Updates the address of a store by its ID. Only company or admin users can perform this operation.";
                    operation.EnsureResponse("200", "Store address updated successfully");
                    operation.EnsureResponse("400", "Update failed");
                    operation.EnsureResponse("401", "Unauthorized");
                    operation.EnsureResponse("500", "Internal server error");
                    return operation;
                });
        group.MapPost("/sellers", CompanyHandlers.RegisterSeller)
            .WithName("RegisterSeller")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a New Seller";
                operation.Description =
                    "Creates a new seller user associated with the company. Sellers are store employees who can process transactions with buyers.\n\n" +
                    "Request requires:\n" +
                    "- username: Username for the new seller\n" +
                    "- email: Email address for the new seller\n" +
                    "- password: Password for the new seller account\n" +
                    "- role: Must be 'Seller' (1)\n\n" +
                    "Successful response returns a success confirmation.";

                operation.EnsureResponse("200", "Seller registered successfully");
                operation.EnsureResponse("400", "Registration failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapPost("/appoint-seller", CompanyHandlers.AppointSeller)
            .WithName("AppointSeller")
            .RequireAuthorization()
            .WithOpenApi(
                operation =>
                {
                    operation.Summary = "Appoint a Seller to a Store";
                    operation.Description =
                        "Appoints an existing seller to a specific store within the company.\n\n" +
                        "Request body requires:\n" +
                        "- sellerId: ID of the seller to appoint\n" +
                        "- storeId: ID of the store to appoint the seller to\n\n" +
                        "Successful response returns a success confirmation.";

                    operation.EnsureResponse("200", "Seller appointed successfully");
                    operation.EnsureResponse("400", "Appointment failed");
                    operation.EnsureResponse("401", "Unauthorized");
                    operation.EnsureResponse("500", "Internal server error");

                    return operation;
                }
            );
        group.MapPost("/remove-seller-from-store", CompanyHandlers.RemoveSellerFromStore)
            .WithName("RemoveSeller")
            .RequireAuthorization()
            .WithOpenApi(
                operation =>
                {
                    operation.Summary = "Remove a Seller from a Store";
                    operation.Description =
                        "Removes a seller from a specific store within the company.\n\n" +
                        "Request body requires:\n" +
                        "- sellerId: ID of the seller to remove\n" +
                        "- storeId: ID of the store to remove the seller from\n\n" +
                        "Successful response returns a success confirmation.";
                    operation.EnsureResponse("200", "Seller removed successfully");
                    operation.EnsureResponse("400", "Removal failed");
                    operation.EnsureResponse("401", "Unauthorized");
                    operation.EnsureResponse("500", "Internal server error");

                    return operation;
                }
            );
        group.MapGet("/statistics", CompanyHandlers.GetStatistics)
            .WithName("GetCompanyStatistics")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Company Statistics";
                operation.Description =
                    "Retrieves dashboard statistics for the company, including bonus circulation, current active bonus, transaction count, and store information.\n\n" +
                    "Query parameters:\n" +
                    "- startDate: Optional. Filter statistics from this date\n" +
                    "- endDate: Optional. Filter statistics to this date\n\n" +
                    "Successful response contains statistics data for the company's dashboard.";

                operation.EnsureResponse("200", "Returns company statistics");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            }); 
        group.MapGet("/ExcelExport", CompanyHandlers.ExportCompanyStoreMetrics)
            .WithName("ExcelExportCompanyStoreMetrics")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Export Company Store Metrics to Excel";
                operation.Description =
                    "Exports the company's store metrics for a specific date to an Excel file. This includes detailed transaction statistics and bonus point activity.\n\n" +
                    "Query parameters:\n" +
                    "- companyId: ID of the company to export metrics for\n" +
                    "- date: Date for which to export metrics\n\n" +
                    "Successful response returns an Excel file containing the store metrics.";

                operation.EnsureResponse("200", "Returns Excel file with store metrics");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapGet("/monitoring", CompanyHandlers.GetMonitoring)
            .WithName("GetCompanyMonitoring")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Company Monitoring Data";
                operation.Description =
                    "Retrieves monitoring data for the company, including transaction statistics and bonus point activity for a specific date.\n\n" +
                    "Query parameters:\n" +
                    "- companyId: ID of the company to monitor\n" +
                    "- companyINN: INN of the company (optional)\n" +
                    "- companyName: Name of the company (optional)\n" +
                    "- date: Date for which to retrieve monitoring data\n\n" +
                    "Successful response contains monitoring data including transaction counts and bonus point activity.";

                operation.EnsureResponse("200", "Returns monitoring data");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            }); 
        group.MapGet("/stores-statistics", CompanyHandlers.GetStoreStatistics)
            .WithName("StoreStats")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Company Store Statistics";
                operation.Description =
                    "Retrieves store statistics for the company, including sales performance and bonus point activity.\n\n" +
                    "Query parameters:\n" +
                    "- companyId: ID of the company to retrieve statistics for\n" +
                    "- startDate: Optional. Start date for the statistics (default: 30 days ago)\n" +
                    "- endDate: Optional. End date for the statistics (default: today)\n\n" +
                    "Successful response contains store statistics data.";

                operation.EnsureResponse("200", "Returns store statistics");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });

        group.MapGet("/transactions", CompanyHandlers.GetTransactionSummary)
            .WithName("GetCompanyTransactionSummary")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Company Transaction Summary";
                operation.Description =
                    "Retrieves a summary of transactions for the company. This includes the most recent transaction details that help the company monitor bonus point activity.\n\n" +
                    "Successful response contains the most recent transaction data or a placeholder if no transactions exist.";

                operation.EnsureResponse("200", "Returns transaction summary");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });

        group.MapPost("/stores-with-sellers", CompanyHandlers.GetStoresWithSellers)
            .WithName("GetStoresWithSellers")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Company Stores with Attached Sellers";
                operation.Description =
                    "Retrieves a paginated list of stores for the company with their attached sellers. Supports filtering by store status and seller role.\n\n" +
                    "Request body parameters:\n" +
                    "- storeStatus: Optional. Filter stores by status (0: PendingApproval, 1: Active, 2: Inactive, 3: Rejected)\n" +
                    "- sellerRole: Optional. Filter sellers by role (should always be 1: Seller)\n" +
                    "- page: Optional. Page number (default: 1)\n" +
                    "- pageSize: Optional. Number of items per page (default: 10, max: 100)\n\n" +
                    "Successful response contains a paginated list of stores with their attached sellers.";

                operation.EnsureResponse("200", "Returns stores with sellers");
                operation.EnsureResponse("400", "Bad request - invalid filter parameters");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapGet("/fiatTransactions", CompanyHandlers.GetFiatTransactionSummary)
            .WithName("GetCompanyFiatTransactionSummary")
            .RequireAuthorization()
            .Produces<FiatTransactionDto>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Company Fiat Transaction Summary";
                operation.Description =
                    "Retrieves a summary of a fiat transactions for the company. This includes the most recent fiat transaction details that help the company monitor bonus point activity.\n\n" +
                    "Successful response contains the most recent fiat transaction data or a placeholder if no transactions exist.";

                operation.EnsureResponse("200", "Returns transaction summary");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });

        // Real-time monitoring endpoints
        app.MapGet("/api/company/{companyId}/statistics/realtime", async (
            Guid companyId,
            ICompanyBffService companyService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                var statistics = await companyService.GetRealTimeStatisticsAsync(companyId);
                return Results.Ok(statistics);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetRealTimeStatistics")
        .WithOpenApi();

        app.MapGet("/api/company/{companyId}/statistics/daily/{date:datetime}", async (
            Guid companyId,
            DateTime date,
            ICompanyBffService companyService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                var statistics = await companyService.GetDailyStatisticsAsync(companyId, date);
                return Results.Ok(statistics);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetDailyStatistics")
        .WithOpenApi();

        app.MapGet("/api/company/{companyId}/statistics/monthly/{year:int}/{month:int}", async (
            Guid companyId,
            int year,
            int month,
            ICompanyBffService companyService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                var statistics = await companyService.GetMonthlyStatisticsAsync(companyId, year, month);
                return Results.Ok(statistics);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetMonthlyStatistics")
        .WithOpenApi();

        app.MapGet("/api/company/{companyId}/statistics/quarterly/{year:int}/{quarter:int}", async (
            Guid companyId,
            int year,
            int quarter,
            ICompanyBffService companyService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                var statistics = await companyService.GetQuarterlyStatisticsAsync(companyId, year, quarter);
                return Results.Ok(statistics);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetQuarterlyStatistics")
        .WithOpenApi();

        app.MapGet("/api/company/{companyId}/statistics/at-date/{date:datetime}", async (
            Guid companyId,
            DateTime date,
            ICompanyBffService companyService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                var statistics = await companyService.GetStatisticsAtDateAsync(companyId, date);
                return Results.Ok(statistics);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetStatisticsAtDate")
        .WithOpenApi();

        app.MapPost("/api/company/{companyId}/monitoring/start", async (
            Guid companyId,
            IRealTimeMonitoringService monitoringService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                await monitoringService.StartRealTimeMonitoringAsync(companyId);
                return Results.Ok(new { message = "Real-time monitoring started successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("StartRealTimeMonitoring")
        .WithOpenApi();

        app.MapPost("/api/company/{companyId}/monitoring/stop", async (
            Guid companyId,
            IRealTimeMonitoringService monitoringService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                await monitoringService.StopRealTimeMonitoringAsync(companyId);
                return Results.Ok(new { message = "Real-time monitoring stopped successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("StopRealTimeMonitoring")
        .WithOpenApi();

        app.MapGet("/api/company/{companyId}/monitoring/status", async (
            Guid companyId,
            ICompanyBffService companyService,
            HttpContext context) =>
        {
            try
            {
                var userId = RequestHelper.GetUserIdFromContext(context);
                if (userId == null)
                    return Results.Unauthorized();

                var isActive = await companyService.GetRealTimeStatisticsAsync(companyId) != null;
                return Results.Ok(new { isActive });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetMonitoringStatus")
        .WithOpenApi();

        return app;
    }
}
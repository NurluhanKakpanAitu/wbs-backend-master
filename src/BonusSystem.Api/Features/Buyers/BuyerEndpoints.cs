using BonusSystem.Api.Infrastructure.Swagger;
using BonusSystem.Shared.Dtos;

namespace BonusSystem.Api.Features.Buyers;

public static class BuyerEndpoints
{
    public static WebApplication MapBuyerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/buyers")
            .RequireAuthorization()
            .WithTags("Buyers")
            .WithOpenApi();

        group.MapGet("/context", BuyerHandlers.GetUserContext)
            .WithName("GetBuyerContext")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Buyer Context";
                operation.Description = "Retrieves the current user's context information including profile data and permitted actions. This is typically called after login to initialize the buyer's dashboard.\n\n" +
                    "Successful response contains:\n" +
                    "- context: User profile information (ID, username, role, bonus balance)\n" +
                    "- actions: List of permitted actions for the buyer";
                
                operation.EnsureResponse("200", "Returns user context and allowed actions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapGet("/get-notifications", BuyerHandlers.GetNotifications)
            .WithName("GetBuyerNotifications")
            .Produces<List<NotificationDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Buyer Notifications";
                operation.Description = "Retrieves notifications for the authenticated buyer.\n\n" +
                    "Each notification record contains:\n" +
                    "- id: Unique notification identifier\n" +
                    "- type: Notification type (0=Transaction, 1=System, 2=Expiration, 3=AdminMessage)\n" +
                    "- timestamp: Date and time of the notification\n" +
                    "- message: Notification message content";                

                operation.EnsureResponse("200", "Returns list of notifications");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");

                return operation;
            });
        group.MapPost("/transfer", BuyerHandlers.TransferBonus)
            .WithName("TransferBonus")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {

                operation.Summary = "Transfer Bonus Points";
                operation.Description = "Transfers bonus points from one user to another. This is typically used for admin actions.\n\n" +
                    "Request body:\n" +
                    "- recipientId: ID of the recipient user\n" +
                    "- amount: Amount of bonus points to transfer\n\n" +
                    "Successful response contains:\n" +
                    "- success: True if the transfer was successful\n" +
                    "- message: Optional success message\n" +
                    "- error: Optional error message\n";

                operation.EnsureResponse("200", "Returns success message");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                return operation;
            }); 
        group.MapGet("/get-transfers", BuyerHandlers.GetTransfers)
            .WithName("GetTransfers")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Transfers";
                return operation; 
            });
        group.MapGet("/get-replenishments", BuyerHandlers.GetReplenishments)
            .WithName("GetReplenishments")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Replenishments";
                return operation; 
            });
        group.MapGet("/categories", BuyerHandlers.GetCategories)
            .WithName("GetCategories")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Categories";
                operation.Description = "Returns a paginated list of store categories. Use for dropdowns or category selection in the UI.\n\n" +
                    "Query parameters:\n" +
                    "- page: Page number (default 1)\n" +
                    "- pageSize: Items per page (default 10)\n\n" +
                    "Successful response contains a paginated list of categories (ID, name).";
                operation.EnsureResponse("200", "Returns list of categories");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                return operation;
            });
        group.MapGet("/get-date-bonus-remove", BuyerHandlers.GetDateBonusRemoved)
            .WithName("GetDateBonusRemove")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Date Bonus Remove";
                operation.Description = "Retrieves the date when bonus points will be removed from the buyer's account. This is used to inform buyers about upcoming expirations.";
                
                operation.EnsureResponse("200", "Returns date bonus remove");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapGet("/balance", BuyerHandlers.GetBalance)
            .WithName("GetBuyerBalance")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Buyer's Bonus Balance";
                operation.Description = "Retrieves the current bonus balance and summary statistics for the authenticated buyer, including total earned, spent, and upcoming expiration information.\n\n" +
                    "Successful response contains:\n" +
                    "- totalEarned: Total bonus points earned by the buyer\n" +
                    "- totalSpent: Total bonus points spent by the buyer\n" +
                    "- currentBalance: Current bonus points balance\n" +
                    "- expiringNextQuarter: Bonus points that will expire at the end of the current quarter\n" +
                    "- recentTransactions: List of recent bonus transactions";
                
                operation.EnsureResponse("200", "Returns bonus balance summary");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/fiat-balance", BuyerHandlers.GetFiatBalance)
            .WithName("GetBuyerFiatBalance")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Buyer's Fiat Balance";
                operation.Description = "Retrieves the current fiat balance for the authenticated buyer.\n\n" +
                    "Each transaction record contains:\n" +
                    "- id: Unique transaction identifier\n" +
                    "- amount: Bonus points amount\n" +
                    "- type: Transaction type (0=Earn, 1=Spend, 2=Expire, 3=AdminAdjustment)\n" +
                    "- timestamp: Date and time of the transaction\n" +
                    "- status: Transaction status (0=Pending, 1=Completed, 2=Reversed, 3=Failed)\n" +
                    "- description: Optional description of the transaction";
                
                operation.EnsureResponse("200", "Returns list of transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapPost("/fiat-replenishment", BuyerHandlers.ReplenishingWallet)
            .WithName("ReplenishFiatBalance")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Replenish Buyer's Fiat Balance";
                operation.Description = "Replenishes the buyer";

                operation.EnsureResponse("200", "Transfer Successful");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                return operation;
            });
        group.MapGet("/qrcode", BuyerHandlers.GenerateQrCode)
            .WithName("GenerateBuyerQrCode")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Generate QR Code for Buyer";
                operation.Description = "Generates a QR code string for the authenticated buyer that can be displayed as an image. This QR code is used for identification at stores when earning or spending bonus points.\n\n" +
                    "Successful response contains:\n" +
                    "- qrCode: String representation of the QR code that can be rendered as an image";
                
                operation.EnsureResponse("200", "Returns QR code");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/stores", BuyerHandlers.FindStores)
            .WithName("FindStoresForBuyer")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Find Stores by Category";
                operation.Description = "Searches for participating stores based on the provided category. Helps buyers find stores where they can earn or spend bonus points.\n\n" +
                    "Query parameters:\n" +
                    "- category: Category of stores to search for (e.g., 'grocery', 'electronics', 'clothing')\n\n" +
                    "Successful response contains a list of stores with their details (ID, name, location, address, status).";
                
                operation.EnsureResponse("200", "Returns list of stores");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/transactions", BuyerHandlers.GetAllTransactions)
            .WithName("GetAllTransactions")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get all transactions for the current user";
                operation.Description = "Retrieves a combined list of all bonus and fiat transactions for the authenticated buyer, sorted by timestamp.";
                operation.EnsureResponse("200", "Returns a list of transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                return operation;
            });
        group.MapGet("/transactionReturns/pendingList", BuyerHandlers.GetPendingReturnsToApprove)
            .WithName("GetPendingReturnsToApprove")
            .RequireAuthorization()
            .Produces<List<TransactionReturnDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get buyer pending transaction returns";
                operation.Description = "Returns list of transaction returns which are requested for buyer to approve\n\n" +
                    "Successful response contains a list of pending transaction returns";
                
                operation.EnsureResponse("200", "Returns list of transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/transactionReturns/approvedList", BuyerHandlers.GetApprovedReturns)
            .WithName("GetApprovedReturnsByUser")
            .Produces<List<TransactionReturnDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get buyer approved transaction returns";
                operation.Description = "Returns list of transaction returns which are approved by the buyer\n\n" +
                    "Successful response contains a list of approved transaction returns";
                
                operation.EnsureResponse("200", "Returns list of transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapPost("/transactionReturns/{id}/rejectReturn", BuyerHandlers.RejectTransactionReturn)
            .WithName("RejectTransactionReturn")
            .Produces<MessageResponseDto>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Reject transaction return";
                operation.Description = "Rejects transaction return that was requested for the buyer to approve\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the transaction return to cancel\n\n" +
                    "Successful response contains a confirmation message.";
                
                operation.EnsureResponse("200", "Transaction return rejected successfully");
                operation.EnsureResponse("400", "Transaction return could not be cancelled");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapPost("/transactionReturns/{id}/approveBonus", BuyerHandlers.ApproveBonusTransactionReturn)
            .WithName("ApproveBonusTransactionReturn")
            .Produces<MessageResponseDto>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Approve bonus transaction return";
                operation.Description = "Approves bonus transaction return and updates company and buyer balances\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the bonus transaction to approve return\n\n" +
                    "Successful response contains a confirmation message.";
                
                operation.EnsureResponse("200", "Transaction return approved successfully");
                operation.EnsureResponse("400", "Transaction return could not be approved");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapPost("/transactionReturns/{id}/approveFiat", BuyerHandlers.ApproveFiatTransactionReturn)
            .WithName("ApproveFiatTransactionReturn")
            .Produces<MessageResponseDto>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Approve fiat transaction return";
                operation.Description = "Approves fiat transaction return and updates company and buyer balances\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the fiat transaction to approve return\n\n" +
                    "Successful response contains a confirmation message.";
                
                operation.EnsureResponse("200", "Transaction return approved successfully");
                operation.EnsureResponse("400", "Transaction return could not be approved");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/fiatTransactions", BuyerHandlers.GetFiatTransactions)
            .WithName("GetBuyerFiatTransactions")
            .Produces<List<FiatTransactionDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Buyer's Fiat Transaction History";
                operation.Description = "Retrieves the complete fiat transaction history for the authenticated buyer.\n\n" +
                    "Each fiat transaction record contains:\n" +
                    "- id: Unique transaction identifier\n" +
                    "- bonusAmount: Bonus points amount\n" +
                    "- totalCost: total deal cost\n" +
                    "- fiatCashBackRate: cashback rate in percent\n" +
                    "- fiatTransactionAmount: fiat transaction amount (after bonuses are applyed)\n" +
                    "- fiatCashBackAmount: fiat cash back amount (after bonuses are applyed, and fiat transaction is made)\n" +
                    "- status: Fiat transaction status  (0=Pending, 1=Completed, 2=Reversed, 3=Failed)\n" +
                    "- timestamp: Date and time of the transaction\n" +
                    "- description: Optional description of the transaction";
                
                
                operation.EnsureResponse("200", "Returns list of transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapPost("/fiatTransactions/{fiatTransactionId:guid}/confirm", BuyerHandlers.ConfirmPendingFiatTransaction)
            .WithName("ConfirmPendingFiatTransaction")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Confirm a pending fiat transaction";
                operation.Description = "Buyer confirms a pending fiat transaction. This triggers the transfer of funds from the buyer to the seller and updates the transaction status to Confirmed.";
                operation.EnsureResponse("200", "Fiat transaction confirmed and funds transferred");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("400", "Bad request");
                operation.EnsureResponse("500", "Internal server error");
                return operation;
            });


        group.MapPost("/bonusTransactions/{transactionId:guid}/confirm", BuyerHandlers.ConfirmPendingBonusTransaction)
            .WithName("ConfirmPendingBonusTransaction")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Confirm a pending bonus transaction";
                operation.Description = "Buyer confirms a pending bonus transaction. This triggers the transfer of bonus points and updates the transaction status to Completed.";
                operation.EnsureResponse("200", "Bonus transaction confirmed and points transferred");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("400", "Bad request");
                operation.EnsureResponse("500", "Internal server error");
                return operation;
            });
        return app;
    }
}

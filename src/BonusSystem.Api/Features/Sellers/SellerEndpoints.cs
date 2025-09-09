using BonusSystem.Api.Infrastructure.Swagger;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.OpenApi.Models;

namespace BonusSystem.Api.Features.Sellers;

public static class SellerEndpoints
{
    public static WebApplication MapSellerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sellers")
            .RequireAuthorization()
            .WithTags("Sellers")
            .WithOpenApi();

        group.MapGet("/context", SellerHandlers.GetUserContext)
            .WithName("GetSellerContext")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Seller Context";
                operation.Description = "Retrieves the seller's user context and permitted actions. This is typically called after login to initialize the seller's interface.\n\n" +
                    "Successful response contains:\n" +
                    "- context: Seller's user context information (ID, username, role)\n" +
                    "- actions: List of actions permitted for the seller";
                
                operation.EnsureResponse("200", "Returns seller context and actions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapGet("/get-company", SellerHandlers.GetCompanyContext)
            .WithName("GetCompanyForSeller")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Company Context";
                
                
                operation.EnsureResponse("200", "Returns seller context and actions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            }); 
        group.MapGet("/get-notifications", SellerHandlers.GetNotifications)
            .WithName("GetSellerNotifications")
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
        group.MapPost("/transactions/process", SellerHandlers.ProcessTransaction)
            .WithName("ProcessTransaction")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Process a Transaction";
                operation.Description = "Creates a new bonus transaction for a buyer at the seller's store. Can be used for both earning (adding) and spending (subtracting) bonus points during a purchase.\n\n" +
                    "Request requires:\n" +
                    "- buyerId: Unique identifier of the buyer (from QR code scan)\n" +
                    "- amount: Bonus amount for the transaction (positive value)\n" +
                    "- type: Transaction type (0=Earn, 1=Spend)\n" +
                    "Successful response contains the processed transaction details.";
                
                operation.EnsureResponse("200", "Transaction processed successfully");
                operation.EnsureResponse("400", "Transaction failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapPost("/transactions/process-combined", SellerHandlers.ProcessCombinedTransaction)
            .WithName("ProcessCombinedTransaction")
            .RequireAuthorization()
            .Produces<CombinedTransactionResultDto>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => 
            {
                operation.Summary = "Process a Combined Transaction";
                operation.Description = "Creates a combined transaction that handles both bonus deduction and fiat payment in a single operation.\n\n" +
                    "Request requires:\n" +
                    "- buyerId: Unique identifier of the buyer (from QR code scan)\n" +
                    "- bonusPercent: Percentage of bonuses to deduct from total cost (0-100)\n" +
                    "- totalCost: Full order cost specified by seller\n" +
                    "- deductBonuses: Whether to deduct bonuses or not (true/false)\n" +
                    "- cashbackPercent: Percentage user gets back as cashback (0-100)\n\n" +
                    "The system automatically calculates:\n" +
                    "- Bonus amount = totalCost × (bonusPercent / 100)\n" +
                    "- Payment amount = totalCost - bonus amount\n" +
                    "- Cashback amount = totalCost × (cashbackPercent / 100)\n" +
                    "- Commission amount = totalCost × (defaultCommissionPercent / 100)\n\n" +
                    "Response contains all calculated values and transaction details.";
                
                operation.EnsureResponse("200", "Combined transaction processed successfully");
                operation.EnsureResponse("400", "Combined transaction failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapPost("/fiatTransactions/process", SellerHandlers.ProcessFiatTransaction)
            .WithName("ProcessFiatTransaction")
            .RequireAuthorization()
            .Produces<List<FiatTransactionResultDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => 
            {
                operation.Summary = "Process a fiat Transaction";
                operation.Description = "Creates a new fiat transaction from the buyer to the seller's store with status of PENDING. The fiat transaction is automatically confirmed and cashback is issued after the 20 minutes\n\n" +
                    "Request requires:\n" +
                    "- buyerId: Unique identifier of the buyer (from QR code scan)\n" +
                    "- bonusAmount: Bonus amount for the transaction (positive value)\n" +
                    "- totalCost: Total cost of fiat transaciton\n" +
                    "- fiatCashBackRate: cashback percentage, can be 0\n" +
                    "Successful response contains the processed fiat transaction details.";
                
                operation.EnsureResponse("200", "Fiat transaction processed successfully");
                operation.EnsureResponse("400", "Fiat transaction failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapPost("/transactionReturns/return", SellerHandlers.RequestTransactionReturn)
            .WithName("RequestTransactionReturn")
            .RequireAuthorization()
            .Produces<TransactionReturnResultDto>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => 
            {
                operation.Summary = "Request transaction return";
                operation.Description = "Requests a transaction return/cancellation that was initiated by a buyer. The buyer must verify and approve the return request.\n\n" +
                    "Request requires:\n" +
                    "- bonusTransactionId: The unique identifier (GUID) of the bonus transaction to return\n\n" +
                    "- fiatTransactionId: The unique identifier (GUID) of the fiat transaction to return\n\n" +
                    "- reason: The reason description for transaction return\n\n" +
                    "Either the fiatTransactionId or the bonusTransactionId must be passed to the endpoint\n\n" +
                    "Successful response contains a confirmation message.";
                
                operation.EnsureResponse("200", "Transaction return confirmed");
                operation.EnsureResponse("400", "Transaction could not be returned");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapGet("/transactionReturns/list", SellerHandlers.GetTransactionReturnsRequestedBySeller)
            .WithName("GetTransactionReturnsRequestedBySeller")
            .Produces<List<TransactionReturnDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get list of transaction returns";
                operation.Description = "Retrieves list of seller's requested transaction returs\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the seller's user id\n\n" +
                    "Successful response contains the list of transaction returns";
                
                operation.EnsureResponse("200", "Returns store's balance");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/buyers/{Id}/balance", SellerHandlers.GetBuyerBalance)
            .WithName("GetBuyerBalanceForSeller")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Buyer's Balance";
                operation.Description = "Retrieves the current bonus balance for a specific buyer. Allows sellers to verify if a buyer has sufficient bonus points for a transaction.\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the buyer\n\n" +
                    "Successful response contains the current bonus balance for the specified buyer.";
                
                operation.EnsureResponse("200", "Returns buyer's balance");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });

        group.MapGet("/balance", SellerHandlers.GetStoreBalance)
            .WithName("GetStoreBalanceForSeller")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Store's Balance";
                operation.Description = "Retrieves the current bonus balance for a specific store. Shows how many bonus points the store has available for distribution to buyers.\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the seller's user id\n\n" +
                    "Successful response contains the current bonus balance for the specified store.";
                
                operation.EnsureResponse("200", "Returns store's balance");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapGet("/fiat_balance", SellerHandlers.GetStoreFiatBalance)
            .WithName("GetStoreFiatBalanceForSeller")
            .RequireAuthorization()
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Store's Fiat Balance";
                operation.Description = "Retrieves the current fiat balance for a specific store. Shows how much fiat currency the store has available.\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the seller's user id\n\n" +
                    "Successful response contains the current fiat balance for the specified store.";
                
                operation.EnsureResponse("200", "Returns store's fiat balance");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapGet("/transactions", SellerHandlers.GetTransactionsForSeller)
            .WithName("GetTransactionsForSeller")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Store's Transactions";

                operation.EnsureResponse("200", "Returns store's transactions");
                operation.EnsureResponse("400", "Transactions could not be retrieved");
                return operation; 
            });

        group.MapGet("/get-buyer-transactions", SellerHandlers.GetTransactionsForBuyer)
            .WithName("GetTransactionsForBuyer")
            .RequireAuthorization()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Buyer's Transactions";
                operation.Description = "Retrieves the buyer's transaction history for a specific store.\n\n";
                operation.EnsureResponse("200", "Returns buyer's transactions");
                operation.EnsureResponse("400", "Transactions could not be retrieved");
                return operation;
            }
            ); 
        group.MapGet("/fiatTransactions/list", SellerHandlers.GetStoreFiatTransactions)
            .WithName("GetStoreFiatTransactionsForSeller")
            .RequireAuthorization()
            .Produces<List<StoreFiatTransactionsDto>>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => 
            {
                operation.Summary = "Get Store's Fiat Transactions";
                operation.Description = "Retrieves the fiat transaction history for a specific store.\n\n" +
                    "Path parameters:\n" +
                    "- id: The unique identifier (GUID) of the seller's user id\n\n" +
                    "Successful response contains a list of transactions with their details.";
                
                operation.EnsureResponse("200", "Returns store's transactions");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                
                return operation;
            });
        group.MapPost("/fiatTransactions/pending", SellerHandlers.CreatePendingFiatTransaction)
            .WithName("CreatePendingFiatTransaction")
            .RequireAuthorization()
            .Produces<FiatTransactionResultDto>(StatusCodes.Status200OK)
            .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create a pending fiat transaction";
                operation.Description = "Creates a new fiat transaction with status Pending. The buyer will receive a notification and must confirm the transaction for the transfer to complete.";
                operation.EnsureResponse("200", "Pending fiat transaction created successfully");
                operation.EnsureResponse("400", "Fiat transaction creation failed");
                operation.EnsureResponse("401", "Unauthorized");
                operation.EnsureResponse("500", "Internal server error");
                return operation;
            });
        return app;
    }
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger;

/// <summary>
/// Enhances schema components in the Swagger document with basic descriptions
/// </summary>
public class SchemaEnhancementFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Ensure components section exists
        if (swaggerDoc.Components?.Schemas == null)
            return;

        // Enhance UserRole enum if it exists
        if (swaggerDoc.Components.Schemas.TryGetValue("UserRole", out var userRoleSchema))
        {
            userRoleSchema.Description = "User role types in the bonus system (0=Buyer, 1=Seller, 2=StoreAdmin, 3=SystemAdmin, 4=CompanyObserver, 5=SystemObserver)";
        }
        
        // Enhance TransactionType enum if it exists
        if (swaggerDoc.Components.Schemas.TryGetValue("TransactionType", out var transactionTypeSchema))
        {
            transactionTypeSchema.Description = "Types of bonus point transactions (0=Earn, 1=Spend, 2=Expire, 3=AdminAdjustment)";
        }
        
        // Enhance TransactionStatus enum if it exists
        if (swaggerDoc.Components.Schemas.TryGetValue("TransactionStatus", out var transactionStatusSchema))
        {
            transactionStatusSchema.Description = "Status values for transactions (0=Pending, 1=Completed, 2=Reversed, 3=Failed)";
        }
        
        // Enhance CompanyStatus enum if it exists
        if (swaggerDoc.Components.Schemas.TryGetValue("CompanyStatus", out var companyStatusSchema))
        {
            companyStatusSchema.Description = "Status values for companies (0=Active, 1=Suspended, 2=Pending)";
        }
        
        // Enhance StoreStatus enum if it exists
        if (swaggerDoc.Components.Schemas.TryGetValue("StoreStatus", out var storeStatusSchema))
        {
            storeStatusSchema.Description = "Status values for stores (0=Active, 1=Inactive, 2=PendingApproval)";
        }
        
        // Enhance NotificationType enum if it exists
        if (swaggerDoc.Components.Schemas.TryGetValue("NotificationType", out var notificationTypeSchema))
        {
            notificationTypeSchema.Description = "Types of notifications (0=Transaction, 1=System, 2=Expiration, 3=AdminMessage)";
        }
        
        // Add descriptions for common DTOs
        AddSchemaDescription(swaggerDoc, "TransactionDto", "A transaction record in the bonus system");
        AddSchemaDescription(swaggerDoc, "TransactionRequestDto", "Request model for creating a new transaction");
        AddSchemaDescription(swaggerDoc, "TransactionResultDto", "Response model containing the result of a transaction operation");
        AddSchemaDescription(swaggerDoc, "BonusTransactionSummaryDto", "Summary of bonus transactions for a user including balance and recent activity");
        AddSchemaDescription(swaggerDoc, "StoreBonusTransactionsDto", "Transaction history for a specific store");
        
        AddSchemaDescription(swaggerDoc, "UserRegistrationDto", "Request model for registering a new user");
        AddSchemaDescription(swaggerDoc, "BuyerRegistrationDto", "Simplified request model for registering a buyer user");
        AddSchemaDescription(swaggerDoc, "UserLoginDto", "Request model for user authentication");
        AddSchemaDescription(swaggerDoc, "UserDto", "Basic user information");
        AddSchemaDescription(swaggerDoc, "UserContextDto", "User context information including role and balance");
        AddSchemaDescription(swaggerDoc, "PermittedActionDto", "An action permitted for the authenticated user");
        
        AddSchemaDescription(swaggerDoc, "CompanyRegistrationDto", "Request model for registering a new company");
        AddSchemaDescription(swaggerDoc, "StoreRegistrationDto", "Request model for registering a new store");
    }
    
    private static void AddSchemaDescription(OpenApiDocument doc, string schemaName, string description)
    {
        if (doc.Components.Schemas.TryGetValue(schemaName, out var schema))
        {
            schema.Description = description;
        }
    }
}

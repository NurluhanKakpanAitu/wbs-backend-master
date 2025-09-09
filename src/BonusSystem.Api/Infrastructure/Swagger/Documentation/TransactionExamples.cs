using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger.Documentation;

/// <summary>
/// Provides example responses for transaction-related API endpoints
/// </summary>
public class TransactionExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodName = context.MethodInfo?.Name;
        
        if (methodName == "ProcessTransaction")
        {
            AddProcessTransactionExample(operation);
        }
        else if (methodName == "GetTransactions" || methodName == "GetBuyerTransactions")
        {
            AddGetTransactionsExample(operation);
        }
        else if (methodName == "GetBalance" || methodName == "GetBuyerBalance")
        {
            AddGetBalanceExample(operation);
        }
        else if (methodName == "ConfirmTransactionReturn")
        {
            AddConfirmTransactionReturnExample(operation);
        }
    }
    
    private static void AddProcessTransactionExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["errorMessage"] = new OpenApiNull(),
                ["transaction"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("99999999-9999-9999-9999-999999999999"),
                    ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                    ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                    ["amount"] = new OpenApiDouble(100),
                    ["type"] = new OpenApiInteger(0), // TransactionType.Earn
                    ["timestamp"] = new OpenApiDateTime(DateTime.UtcNow),
                    ["status"] = new OpenApiInteger(1), // TransactionStatus.Completed
                    ["description"] = new OpenApiString("Bonus points earned for purchase")
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(false),
                ["errorMessage"] = new OpenApiString("Insufficient bonus balance for spending transaction"),
                ["transaction"] = new OpenApiNull()
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddGetTransactionsExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("99999999-9999-9999-9999-999999999999"),
                    ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                    ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                    ["amount"] = new OpenApiDouble(100),
                    ["type"] = new OpenApiInteger(0), // TransactionType.Earn
                    ["timestamp"] = new OpenApiDateTime(DateTime.UtcNow.AddDays(-1)),
                    ["status"] = new OpenApiInteger(1), // TransactionStatus.Completed
                    ["description"] = new OpenApiString("Bonus points earned for purchase")
                },
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("88888888-8888-8888-8888-888888888888"),
                    ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                    ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                    ["amount"] = new OpenApiDouble(50),
                    ["type"] = new OpenApiInteger(1), // TransactionType.Spend
                    ["timestamp"] = new OpenApiDateTime(DateTime.UtcNow),
                    ["status"] = new OpenApiInteger(1), // TransactionStatus.Completed
                    ["description"] = new OpenApiString("Bonus points spent on discount")
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddGetBalanceExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["totalEarned"] = new OpenApiDouble(1500),
                ["totalSpent"] = new OpenApiDouble(800),
                ["currentBalance"] = new OpenApiDouble(700),
                ["expiringNextQuarter"] = new OpenApiDouble(200),
                ["recentTransactions"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("99999999-9999-9999-9999-999999999999"),
                        ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                        ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                        ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                        ["amount"] = new OpenApiDouble(100),
                        ["type"] = new OpenApiInteger(0), // TransactionType.Earn
                        ["timestamp"] = new OpenApiDateTime(DateTime.UtcNow.AddDays(-1)),
                        ["status"] = new OpenApiInteger(1), // TransactionStatus.Completed
                        ["description"] = new OpenApiString("Bonus points earned for purchase")
                    },
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("88888888-8888-8888-8888-888888888888"),
                        ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                        ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                        ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                        ["amount"] = new OpenApiDouble(50),
                        ["type"] = new OpenApiInteger(1), // TransactionType.Spend
                        ["timestamp"] = new OpenApiDateTime(DateTime.UtcNow),
                        ["status"] = new OpenApiInteger(1), // TransactionStatus.Completed
                        ["description"] = new OpenApiString("Bonus points spent on discount")
                    }
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddConfirmTransactionReturnExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Transaction return confirmed successfully")
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Transaction could not be returned")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
}
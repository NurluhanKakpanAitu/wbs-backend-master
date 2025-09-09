using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger.Documentation;

/// <summary>
/// Provides example responses for company and store-related API endpoints
/// </summary>
public class CompanyStoreExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodName = context.MethodInfo?.Name;
        
        if (methodName == "FindStores" || methodName == "FindStoresForBuyer")
        {
            AddFindStoresExample(operation);
        }
        else if (methodName == "GetStoreTransactions" || methodName == "GetStoreTransactionsForSeller")
        {
            AddGetStoreTransactionsExample(operation);
        }
        else if (methodName == "GetStoreBalance" || methodName == "GetStoreBalanceForSeller")
        {
            AddGetStoreBalanceExample(operation);
        }
    }
    
    private static void AddFindStoresExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                    ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["name"] = new OpenApiString("Sample Store"),
                    ["location"] = new OpenApiString("Downtown"),
                    ["address"] = new OpenApiString("123 Main St, Anytown, AN 12345"),
                    ["contactPhone"] = new OpenApiString("+1-555-987-6543"),
                    ["status"] = new OpenApiInteger(0) // StoreStatus.Active
                },
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["name"] = new OpenApiString("Uptown Store"),
                    ["location"] = new OpenApiString("Uptown"),
                    ["address"] = new OpenApiString("456 High St, Anytown, AN 12345"),
                    ["contactPhone"] = new OpenApiString("+1-555-456-7890"),
                    ["status"] = new OpenApiInteger(0) // StoreStatus.Active
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddGetStoreTransactionsExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                ["storeName"] = new OpenApiString("Sample Store"),
                ["totalTransactions"] = new OpenApiDouble(2),
                ["transactions"] = new OpenApiArray
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
    
    private static void AddGetStoreBalanceExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["balance"] = new OpenApiDouble(25000)
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
}
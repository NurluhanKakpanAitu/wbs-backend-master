using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger.Documentation;

/// <summary>
/// Provides example responses for authentication-related API endpoints
/// </summary>
public class AuthExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodName = context.MethodInfo?.Name;

        if (methodName == "Register" || methodName == "BuyerRegister")
        {
            AddRegistrationExample(operation);
        }
        else if (methodName == "Login")
        {
            AddLoginExample(operation);
        }
        else if (methodName == "GetUserContext" || methodName == "GetBuyerContext" || methodName == "GetSellerContext")
        {
            AddUserContextExample(operation, methodName);
        } 
        else if (methodName == "VerifyEmail" )
        {
            VerifyUser(operation);
        }
        else if (methodName == "LoginAdmin")
        {
            AddLoginExample(operation);
        }
        else if (methodName == "UpdateBuyer")
        {
            AddUserContextExample(operation, methodName);
        }
        else if (methodName == "UpdateSeller")
        {
            AddUserContextExample(operation, methodName);
        }
        else if (methodName == "UpdateAdmin")
        {
            AddUserContextExample(operation, methodName);
        }
    }
    
    private static void AddRegistrationExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("User registered successfully")
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("User with this email already exists")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddLoginExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Login successful")
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Invalid email or password")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    private static void VerifyUser(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("User verified successfully"),
                ["token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTExMTExMS0xMTExLTExMTEtMTExMS0xMTExMTExMTExMTEiLCJqdGkiOiJmMzUzNDgxYS03NmJmLTQ0YzMtOTk5NC0xNGJlMGE3ZGY1YzYiLCJlbWFpbCI6ImJ1eWVyMUBleGFtcGxlLmNvbSIsInJvbGUiOiJCdXllciIsIm5iZiI6MTY0NjU5NjgxMCwiZXhwIjoxNjQ2NjgzMjEwLCJpYXQiOjE2NDY1OTY4MTAsImlzcyI6ImJvbnVzLXN5c3RlbS1hcGkiLCJhdWQiOiJib251cy1zeXN0ZW0tY2xpZW50In0.qPYs49HM5JpCzLGQVk2RMxmV2oJG2YQCmVmCjwJYvAo"),
                ["role"] = new OpenApiInteger(0),
                ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111")
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Verification failed: Invalid token")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    private static void AddUserContextExample(OpenApiOperation operation, string methodName)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var contextExample = new OpenApiObject
            {
                ["context"] = new OpenApiObject
                {
                    ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                    ["username"] = new OpenApiString(methodName.Contains("Buyer") ? "buyer1" : 
                                                    methodName.Contains("Seller") ? "seller1" : "admin1"),
                    ["role"] = new OpenApiInteger(methodName.Contains("Buyer") ? 0 : 
                                                 methodName.Contains("Seller") ? 1 : 3), // UserRole enum value
                    ["bonusBalance"] = new OpenApiDouble(methodName.Contains("Buyer") ? 700 : 0)
                },
                ["actions"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("ViewTransactions"),
                        ["description"] = new OpenApiString("View transaction history"),
                        ["endpoint"] = new OpenApiString(methodName.Contains("Buyer") ? "/api/buyers/transactions" : 
                                                        methodName.Contains("Seller") ? "/api/sellers/stores/{id}/transactions" : 
                                                        "/api/admin/transactions")
                    },
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("ViewBalance"),
                        ["description"] = new OpenApiString("View bonus balance"),
                        ["endpoint"] = new OpenApiString(methodName.Contains("Buyer") ? "/api/buyers/balance" : 
                                                        methodName.Contains("Seller") ? "/api/sellers/stores/{id}/balance" : 
                                                        "/api/admin/companies/{id}/balance")
                    }
                }
            };
            
            successResponse.Content["application/json"].Example = contextExample;
        }
    }
}
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger.Documentation;

/// <summary>
/// Provides example responses for admin-related API endpoints
/// </summary>
public class AdminExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodName = context.MethodInfo?.Name;
        
        if (methodName == "GetAdminContext" || methodName == "GetUserContext")
        {
            AddAdminContextExample(operation);
        }
        else if (methodName == "RegisterCompany")
        {
            AddRegisterCompanyExample(operation);
        }
        else if (methodName == "UpdateCompanyStatus")
        {
            AddUpdateCompanyStatusExample(operation);
        }
        else if (methodName == "ModerateStore")
        {
            AddModerateStoreExample(operation);
        }
        else if (methodName == "CreditCompanyBalance")
        {
            AddCreditCompanyBalanceExample(operation);
        }
        else if (methodName == "GetSystemTransactions")
        {
            AddGetSystemTransactionsExample(operation);
        }
        else if (methodName == "SendSystemNotification")
        {
            AddSendSystemNotificationExample(operation);
        }
    }
    
    private static void AddAdminContextExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["context"] = new OpenApiObject
                {
                    ["userId"] = new OpenApiString("33333333-3333-3333-3333-333333333333"),
                    ["username"] = new OpenApiString("admin1"),
                    ["role"] = new OpenApiInteger(3), // UserRole.SystemAdmin
                    ["bonusBalance"] = new OpenApiDouble(0)
                },
                ["actions"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("RegisterCompany"),
                        ["description"] = new OpenApiString("Register a new company"),
                        ["endpoint"] = new OpenApiString("/api/admin/companies")
                    },
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("UpdateCompanyStatus"),
                        ["description"] = new OpenApiString("Update company status"),
                        ["endpoint"] = new OpenApiString("/api/admin/companies/{id}/status")
                    },
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("ModerateStore"),
                        ["description"] = new OpenApiString("Approve or reject store registration"),
                        ["endpoint"] = new OpenApiString("/api/admin/stores/{id}/moderate")
                    },
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("CreditCompanyBalance"),
                        ["description"] = new OpenApiString("Add bonus balance to a company"),
                        ["endpoint"] = new OpenApiString("/api/admin/companies/{id}/credit")
                    }
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddRegisterCompanyExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["errorMessage"] = new OpenApiNull(),
                ["company"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["name"] = new OpenApiString("Sample Company"),
                    ["contactEmail"] = new OpenApiString("contact@samplecompany.com"),
                    ["contactPhone"] = new OpenApiString("+1-555-123-4567"),
                    ["bonusBalance"] = new OpenApiDouble(1000000),
                    ["originalBonusBalance"] = new OpenApiDouble(1000000),
                    ["status"] = new OpenApiInteger(0), // CompanyStatus.Active
                    ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow),
                    ["stores"] = new OpenApiArray()
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(false),
                ["errorMessage"] = new OpenApiString("Company with this name already exists"),
                ["company"] = new OpenApiNull()
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddUpdateCompanyStatusExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Company status updated successfully"),
                ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                ["newStatus"] = new OpenApiInteger(1) // CompanyStatus.Suspended
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Invalid company status value")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddModerateStoreExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Store has been approved and activated"),
                ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                ["status"] = new OpenApiInteger(0) // StoreStatus.Active
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Store is not in a pending status")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddCreditCompanyBalanceExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Company balance credited successfully"),
                ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                ["previousBalance"] = new OpenApiDouble(1000000),
                ["creditAmount"] = new OpenApiDouble(500000),
                ["newBalance"] = new OpenApiDouble(1500000)
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Credit amount must be positive")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddGetSystemTransactionsExample(OpenApiOperation operation)
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
                },
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("77777777-8888-9999-aaaa-bbbbbbbbbbbb"),
                    ["userId"] = new OpenApiString("22222222-2222-2222-2222-222222222222"),
                    ["companyId"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                    ["amount"] = new OpenApiDouble(75),
                    ["type"] = new OpenApiInteger(0), // TransactionType.Earn
                    ["timestamp"] = new OpenApiDateTime(DateTime.UtcNow.AddDays(-2)),
                    ["status"] = new OpenApiInteger(1), // TransactionStatus.Completed
                    ["description"] = new OpenApiString("Bonus points earned for purchase")
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddSendSystemNotificationExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Notification sent successfully"),
                ["recipientCount"] = new OpenApiInteger(3),
                ["notificationId"] = new OpenApiString("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
            };
            
            successResponse.Content["application/json"].Example = example;
        }
        
        if (operation.Responses.TryGetValue("400", out var errorResponse))
        {
            var example = new OpenApiObject
            {
                ["message"] = new OpenApiString("Message content cannot be empty")
            };
            
            errorResponse.Content["application/json"].Example = example;
        }
    }
}
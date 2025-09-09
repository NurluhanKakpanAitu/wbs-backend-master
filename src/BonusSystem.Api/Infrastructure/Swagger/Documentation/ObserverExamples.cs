using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusSystem.Api.Infrastructure.Swagger.Documentation;

/// <summary>
/// Provides example responses for observer-related API endpoints
/// </summary>
public class ObserverExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var methodName = context.MethodInfo?.Name;
        
        if (methodName == "GetObserverContext" || (methodName == "GetUserContext" && operation.OperationId?.Contains("Observer") == true))
        {
            AddObserverContextExample(operation);
        }
        else if (methodName == "GetObserverStatistics" || methodName == "GetStatistics")
        {
            AddGetStatisticsExample(operation);
        }
        else if (methodName == "GetObserverTransactionSummary" || methodName == "GetTransactionSummary")
        {
            AddGetTransactionSummaryExample(operation);
        }
        else if (methodName == "GetObserverCompaniesOverview" || methodName == "GetCompaniesOverview")
        {
            AddGetCompaniesOverviewExample(operation);
        }
    }
    
    private static void AddObserverContextExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["context"] = new OpenApiObject
                {
                    ["userId"] = new OpenApiString("55555555-5555-5555-5555-555555555555"),
                    ["username"] = new OpenApiString("observer1"),
                    ["role"] = new OpenApiInteger(4), // UserRole.CompanyObserver
                    ["bonusBalance"] = new OpenApiDouble(0)
                },
                ["actions"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("ViewStatistics"),
                        ["description"] = new OpenApiString("View system statistics"),
                        ["endpoint"] = new OpenApiString("/api/observers/statistics")
                    },
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("ViewTransactionSummary"),
                        ["description"] = new OpenApiString("View transaction summary"),
                        ["endpoint"] = new OpenApiString("/api/observers/transactions/summary")
                    },
                    new OpenApiObject
                    {
                        ["actionName"] = new OpenApiString("ViewCompaniesOverview"),
                        ["description"] = new OpenApiString("View companies overview"),
                        ["endpoint"] = new OpenApiString("/api/observers/companies")
                    }
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddGetStatisticsExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["periodStart"] = new OpenApiDateTime(DateTime.UtcNow.AddMonths(-1)),
                ["periodEnd"] = new OpenApiDateTime(DateTime.UtcNow),
                ["totalTransactions"] = new OpenApiInteger(1245),
                ["uniqueActiveUsers"] = new OpenApiInteger(387),
                ["totalBonusesEarned"] = new OpenApiDouble(127850),
                ["totalBonusesSpent"] = new OpenApiDouble(98750),
                ["totalBonusesExpired"] = new OpenApiDouble(5600),
                ["averageTransactionAmount"] = new OpenApiDouble(102.69),
                ["storeStatistics"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["storeId"] = new OpenApiString("77777777-7777-7777-7777-777777777777"),
                        ["storeName"] = new OpenApiString("Sample Store"),
                        ["transactionCount"] = new OpenApiInteger(350),
                        ["uniqueUsers"] = new OpenApiInteger(145),
                        ["totalEarned"] = new OpenApiDouble(35000),
                        ["totalSpent"] = new OpenApiDouble(28500)
                    },
                    new OpenApiObject
                    {
                        ["storeId"] = new OpenApiString("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        ["storeName"] = new OpenApiString("Uptown Store"),
                        ["transactionCount"] = new OpenApiInteger(275),
                        ["uniqueUsers"] = new OpenApiInteger(120),
                        ["totalEarned"] = new OpenApiDouble(27500),
                        ["totalSpent"] = new OpenApiDouble(23000)
                    }
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddGetTransactionSummaryExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiObject
            {
                ["totalTransactions"] = new OpenApiInteger(1245),
                ["byType"] = new OpenApiObject
                {
                    ["Earn"] = new OpenApiInteger(720),
                    ["Spend"] = new OpenApiInteger(425),
                    ["Expire"] = new OpenApiInteger(85),
                    ["AdminAdjustment"] = new OpenApiInteger(15)
                },
                ["byStatus"] = new OpenApiObject
                {
                    ["Pending"] = new OpenApiInteger(12),
                    ["Completed"] = new OpenApiInteger(1210),
                    ["Reversed"] = new OpenApiInteger(8),
                    ["Failed"] = new OpenApiInteger(15)
                },
                ["volumeByDay"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["date"] = new OpenApiString(DateTime.UtcNow.AddDays(-6).ToString("yyyy-MM-dd")),
                        ["count"] = new OpenApiInteger(156)
                    },
                    new OpenApiObject
                    {
                        ["date"] = new OpenApiString(DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd")),
                        ["count"] = new OpenApiInteger(172)
                    },
                    new OpenApiObject
                    {
                        ["date"] = new OpenApiString(DateTime.UtcNow.AddDays(-4).ToString("yyyy-MM-dd")),
                        ["count"] = new OpenApiInteger(189)
                    },
                    new OpenApiObject
                    {
                        ["date"] = new OpenApiString(DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd")),
                        ["count"] = new OpenApiInteger(178)
                    },
                    new OpenApiObject
                    {
                        ["date"] = new OpenApiString(DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd")),
                        ["count"] = new OpenApiInteger(201)
                    },
                    new OpenApiObject
                    {
                        ["date"] = new OpenApiString(DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd")),
                        ["count"] = new OpenApiInteger(197)
                    },
                    new OpenApiObject
                    {
                        ["date"] = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-dd")),
                        ["count"] = new OpenApiInteger(152)
                    }
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
    
    private static void AddGetCompaniesOverviewExample(OpenApiOperation operation)
    {
        if (operation.Responses.TryGetValue("200", out var successResponse))
        {
            var example = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("66666666-6666-6666-6666-666666666666"),
                    ["name"] = new OpenApiString("Sample Company"),
                    ["bonusBalance"] = new OpenApiDouble(875000),
                    ["transactionVolume"] = new OpenApiInteger(625),
                    ["storeCount"] = new OpenApiInteger(2),
                    ["status"] = new OpenApiInteger(0), // CompanyStatus.Active
                    ["statistics"] = new OpenApiObject
                    {
                        ["totalEarned"] = new OpenApiDouble(62500),
                        ["totalSpent"] = new OpenApiDouble(51500),
                        ["activeUsers"] = new OpenApiInteger(265)
                    }
                },
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("77777777-6666-5555-4444-333333333333"),
                    ["name"] = new OpenApiString("Another Company"),
                    ["bonusBalance"] = new OpenApiDouble(1250000),
                    ["transactionVolume"] = new OpenApiInteger(620),
                    ["storeCount"] = new OpenApiInteger(3),
                    ["status"] = new OpenApiInteger(0), // CompanyStatus.Active
                    ["statistics"] = new OpenApiObject
                    {
                        ["totalEarned"] = new OpenApiDouble(65350),
                        ["totalSpent"] = new OpenApiDouble(47250),
                        ["activeUsers"] = new OpenApiInteger(122)
                    }
                }
            };
            
            successResponse.Content["application/json"].Example = example;
        }
    }
}
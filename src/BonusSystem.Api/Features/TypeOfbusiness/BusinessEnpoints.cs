
namespace BonusSystem.Api.Features.TypeOfbusiness;

using System.Collections.Generic;
using System.Threading;

public static class BusinessEndpoints
{
    public static WebApplication MapBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/business").WithTags("Business");
        group.MapGet("/", BusinessHandlers.HandleGetTypeOfBusinessAsync)
            .WithName("GetTypeOfBusiness")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get list of business types";
                operation.Description = "Returns a list of available business types for dropdown selection.";
                return operation;
            });
        return app;
    }
    
}
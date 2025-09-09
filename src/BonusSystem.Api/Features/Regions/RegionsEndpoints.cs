
namespace BonusSystem.Api.Features.Regions;

public static class RegionEndpoints
{
    public static WebApplication MapRegionsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/regions").WithTags("Regions").WithOpenApi();
        group.MapGet("/all", RegionsHandlers.GetRegions)
            .WithName("GetRegions")
            .AllowAnonymous()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get list of regions";
                operation.Description = "Returns a list of available regions";
                return operation; 
            });
        group.MapPost("/get/by-id/{id:int}", RegionsHandlers.GetRegionById)
            .WithName("GetRegionById")
            .AllowAnonymous()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get Region by id";
                return operation;
            });

        group.MapPost("/get/by-name/{name}", RegionsHandlers.GetRegionByName)
            .WithName("GetRegionByName")
            .AllowAnonymous()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get region by name";
                return operation;
            });

        return app; 
    }
}
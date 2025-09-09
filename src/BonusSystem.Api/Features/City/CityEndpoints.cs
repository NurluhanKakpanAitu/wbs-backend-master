
namespace BonusSystem.Api.Features.City;

public static class CityEndpoints
{
    public static WebApplication MapCityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cities").WithTags("Cities").WithOpenApi();
        group.MapGet("/all", CityHandlers.GetCities)
            .AllowAnonymous()
            .WithName("GetCities")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get list of cities";
                operation.Description = "Returns a list of available cities for dropdown selection.";
                return operation;
            });
        group.MapPost("/get/by-id/{id:int}", CityHandlers.GetCityById)
            .WithName("GetCityById")
            .AllowAnonymous()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get City by id";
                return operation;
            });

        group.MapPost("/get/by-name/{name}", CityHandlers.GetCityByName)
            .WithName("GetCityByName")
            .AllowAnonymous()
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get City by name";
                return operation;
            });
        return app;
    }
}
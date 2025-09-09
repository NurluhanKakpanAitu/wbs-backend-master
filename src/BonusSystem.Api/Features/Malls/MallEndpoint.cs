
using BonusSystem.Api.Infrastructure.Swagger;

namespace BonusSystem.Api.Features.Malls;

public static class MallEnpoints
{
    public static WebApplication MapMallsEndpoints(WebApplication app)
    {
        var group = app.MapGroup("api/malls")
            .WithTags("Malls")
            .WithOpenApi();

        group.MapGet("/get-all", MallHandlers.GetMalls)
            .WithName("AllMalls")
            .AllowAnonymous()
            .WithOpenApi( 
                operation =>
                {
                    operation.Summary = "Get all malls";
                    operation.Description = "Get all malls";
                    operation.EnsureResponse("200", "Malls");
                    operation.EnsureResponse("400", "Error");  
                    return operation; 
                } 
            );

        group.MapGet("/get-by-id/{id}", MallHandlers.GetMallById)
            .WithName("MallById")
            .AllowAnonymous()
            .WithOpenApi(
                operation =>
                {
                    operation.Summary = "Get mall by id";
                    operation.Description = "Get mall by id";
                    operation.EnsureResponse("200", "Mall");
                    operation.EnsureResponse("400", "Error");
                    return operation;
                }
            ); 
        
        group.MapGet("/get-by-id/{id}", MallHandlers.GetMallById)
            .WithName("MallById")
            .AllowAnonymous()
            .WithOpenApi(
                operation =>
                {
                    operation.Summary = "Get mall by id";
                    operation.Description = "Get mall by id";
                    operation.EnsureResponse("200", "Mall");
                    operation.EnsureResponse("400", "Error");
                    return operation;
                }
            ); 
        group.MapGet("/get-by-id/{name}", MallHandlers.GetMallById)
            .WithName("MallByName")
            .AllowAnonymous()
            .WithOpenApi(
                operation =>
                {
                    operation.Summary = "Get mall by name";
                    operation.Description = "Get mall by name";
                    operation.EnsureResponse("200", "Mall");
                    operation.EnsureResponse("400", "Error");
                    return operation;
                }
            ); 
        return app; 
    }
} 
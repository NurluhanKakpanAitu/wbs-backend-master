
using Microsoft.AspNetCore.Mvc;
using BonusSystem.Core.Services.Interfaces; 

namespace BonusSystem.Api.Features.Regions;

public static class RegionsHandlers
{
    public static async Task<IResult> GetRegions(
        [FromServices] IRegionBffService regionService
    )
    {
        var regions = await regionService.GetRegionsAsync();
        return Results.Ok(regions);
    }
    public static async Task<IResult> GetRegionByName(
        [FromServices] IRegionBffService regionService,
        string Name
    )
    {
        var region = await regionService.GetRegionByNameAsync(Name);
        return Results.Ok(region);
    }
    public static async Task<IResult> GetRegionById(
        [FromServices] IRegionBffService regionService,
        int Id
    )
    {
        var region = await regionService.GetRegionByIdAsync(Id);
        return Results.Ok(region); 
    }
}
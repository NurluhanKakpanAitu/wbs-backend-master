using BonusSystem.Core.Services.Interfaces.BFF;
using BonusSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonusSystem.Api.Features.City;

public static class CityHandlers
{
    public static async Task<IResult> GetCities(
        [FromServices] ICityService cityService
    )
    {
        var cities = await cityService.GetCitiesAsync();
        return Results.Ok(cities);
    }
    public static async Task<IResult> GetCityByName(
        [FromServices] ICityService CityService,
        string Name
    )
    {
        var City = await CityService.GetCityByNameAsync(Name);
        return Results.Ok(City);
    }
    public static async Task<IResult> GetCityById(
        [FromServices] ICityService CityService,
        int Id
    )
    {
        var City = await CityService.GetCityByIdAsync(Id);
        return Results.Ok(City); 
    }
}
using BonusSystem.Shared.Dtos;
namespace BonusSystem.Core.Services.Interfaces.BFF;

public interface ICityService
{
    Task<IEnumerable<CityDto>> GetCitiesAsync();
    Task<CityDto> GetCityByIdAsync(int id);
    Task<CityDto> GetCityByNameAsync(string name); 
}
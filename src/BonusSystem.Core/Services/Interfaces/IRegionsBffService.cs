using BonusSystem.Shared.Dtos; 
namespace BonusSystem.Core.Services.Interfaces;

public interface IRegionBffService
{
    Task<IEnumerable<RegionsDto>> GetRegionsAsync();
    Task<RegionsDto> GetRegionByIdAsync(int id);
    Task<RegionsDto> GetRegionByNameAsync(string name); 
}
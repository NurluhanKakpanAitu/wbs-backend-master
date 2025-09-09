using BonusSystem.Shared.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonusSystem.Core.Services.Interfaces.BFF;
using BonusSystem.Core.Services.Interfaces;

namespace BonusSystem.Core.Services.Implementations.BFF;

public class RegionsBffService : IRegionBffService
{
    private static readonly List<RegionsDto> _regions = new()
    {
            new () { Id = 1, Name = "Абайская область" },
            new() { Id = 2, Name = "Актюбинская область" },
            new() { Id = 3, Name = "Алматинская область" },
            new() { Id = 4, Name = "Атырауская область" },
            new() { Id = 5, Name = "Восточно-Казахстанская область" },
            new() { Id = 6, Name = "Жамбылская область" },
            new() { Id = 7, Name = "Жетысуская область" },
            new() { Id = 8, Name = "Западно-Казахстанская область" },
            new() { Id = 9, Name = "Карагандинская область" },
            new() { Id = 10, Name = "Костанайская область" },
            new() { Id = 11, Name = "Кызылординская область" },
            new() { Id = 12, Name = "Мангистауская область" },
            new() { Id = 13, Name = "Павлодарская область" },
            new() { Id = 14, Name = "Северо-Казахстанская область" },
            new() { Id = 15, Name = "Туркестанская область" },
            new() { Id = 16, Name = "Улытауская область" },
            new() { Id = 17, Name = "Астана" }, // город республиканского значения
            new() { Id = 18, Name = "Алматы" }, // город республиканского значения
            new() { Id = 19, Name = "Шымкент" }  // город республиканского значения 
    };
    public async Task<IEnumerable<RegionsDto>> GetRegionsAsync()
    {
        return await Task.FromResult(_regions);
    }
    public async Task<RegionsDto> GetRegionByIdAsync(int id)
    {
        var region = _regions.FirstOrDefault(r => r.Id == id);
        return await Task.FromResult(region);
    }
    public async Task<RegionsDto> GetRegionByNameAsync(string name)
    {
        var region = _regions.FirstOrDefault(r => r.Name == name);
        return await Task.FromResult(region); 
    }
}
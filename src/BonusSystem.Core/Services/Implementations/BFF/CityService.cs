using BonusSystem.Shared.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonusSystem.Core.Services.Interfaces.BFF;

namespace BonusSystem.Core.Services.Implementations.BFF;

public class CityService : ICityService
{
    private static readonly List<CityDto> _cities = new()
    {
        // Города республиканского значения
        new() { Id = 1, Name = "Астана" },      // столица
        new() { Id = 2, Name = "Алматы" },      // крупнейший город
        new() { Id = 3, Name = "Шымкент" },     // город-миллионник 
        // Областные центры
        new() { Id = 4, Name = "Семей" },             // Абайская область
        new() { Id = 5, Name = "Кокшетау" },          // Акмолинская область
        new() { Id = 6, Name = "Актобе" },            // Актюбинская область
        new() { Id = 7, Name = "Қонаев" },            // Алматинская область
        new() { Id = 8, Name = "Атырау" },            // Атырауская область
        new() { Id = 9, Name = "Өскемен" },           // Восточно-Казахстанская область
        new() { Id = 10, Name = "Тараз" },            // Жамбылская область
        new() { Id = 11, Name = "Талдыкорган" },      // Жетысуская область
        new() { Id = 12, Name = "Орал" },             // Западно-Казахстанская область
        new() { Id = 13, Name = "Караганда" },        // Карагандинская область
        new() { Id = 14, Name = "Костанай" },         // Костанайская область
        new() { Id = 15, Name = "Кызылорда" },        // Кызылординская область
        new() { Id = 16, Name = "Актау" },            // Мангистауская область
        new() { Id = 17, Name = "Павлодар" },         // Павлодарская область
        new() { Id = 18, Name = "Петропавловск" },    // Северо-Казахстанская область
        new() { Id = 19, Name = "Туркестан" },        // Туркестанская область
        new() { Id = 20, Name = "Жезказган" }         // Улытауская область
    };
    public async Task<IEnumerable<CityDto>> GetCitiesAsync()
    {
        return await Task.FromResult(_cities);
    }
    public async Task<CityDto> GetCityByIdAsync(int id)
    {
        var city = _cities.FirstOrDefault(c => c.Id == id);
        return await Task.FromResult(city);
    }
    public async Task<CityDto> GetCityByNameAsync(string name)
    {
        var city = _cities.FirstOrDefault(c => c.Name == name);
        return await Task.FromResult(city); 
    }
}


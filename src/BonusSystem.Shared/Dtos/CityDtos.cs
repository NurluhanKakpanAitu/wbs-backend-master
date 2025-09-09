
namespace BonusSystem.Shared.Dtos;

public record CityDto
{
    public int Id { get; init; }
    public string Name { get; init; }
}

public record RegionsDto
{
    public int Id { get; init; }
    public string Name { get; init; }
}

namespace BonusSystem.Shared.Dtos;

public record MallDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string WorkHours { get; init; } = string.Empty;
    public int City { get; init; }
    public int Region { get; init; }
}

public record MallRegister
{ 
    public string Name { get; init; } = string.Empty;
    public string WorkHours { get; init; } = string.Empty;
    public int City { get; init; }
    public int Region { get; init; }
}
namespace BonusSystem.Shared.Dtos;

public record StoreFilterRequestDto
{
    public int CategoryId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
} 
public record CategoryDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
}
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
public class TypeOfBusiness
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}

public record CompanyStoresDto
{
    public required string Name { get; init; }
    public List<StoreDto> Stores { get; init; } = new();
}
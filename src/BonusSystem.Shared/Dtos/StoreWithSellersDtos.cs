using BonusSystem.Shared.Models;

namespace BonusSystem.Shared.Dtos;

/// <summary>
/// DTO for store with attached sellers
/// </summary>
public record StoreWithSellersDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; }
    public Guid CompanyId { get; init; }
    public int BusinessTypeId { get; init; }
    public int CategoryId { get; init; }
    public string MallId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int City { get; init; }
    public int Region { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Floor { get; init; } = string.Empty;
    public string Row { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string WorkingHours { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public string TypeOfbusiness { get; set; } = string.Empty;
    public List<Guid> SellerIds { get; init; } = new();
    public StoreStatus Status { get; init; }
}

/// <summary>
/// DTO for paginated response of stores with sellers
/// </summary>
public record StoresWithSellersPagedResponseDto
{
    public List<StoreWithSellersDto> Stores { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// DTO for filtering stores and sellers
/// </summary>
public record StoresFilterRequestDto
{
    public StoreStatus? StoreStatus { get; init; }
    public UserRole? SellerRole { get; init; } // Always Seller, but included for potential future extensions
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}


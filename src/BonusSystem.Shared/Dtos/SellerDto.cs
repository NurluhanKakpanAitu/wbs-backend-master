
namespace BonusSystem.Shared.Dtos;

public record SellerDto
{
    public Guid Id { get; init; }
    public string FrontendId { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; } 
    public string Address { get; init; }
    public string BIK { get; init; }
}

public record SellerUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string BIK { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}

public record AppointSellerDto
{
    public Guid SellerId { get; init; }
    public Guid StoreId { get; init; }
}

public record SellerOutput
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string StoreName { get; init; } 
    public bool IsAppoint { get; init;  }
}
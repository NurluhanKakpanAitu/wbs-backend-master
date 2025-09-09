using BonusSystem.Shared.Models;
namespace BonusSystem.Shared.Dtos;


public class UserDto
{
    public Guid Id { get; set; }
    public string FrontendId { get; init; } 
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; } 
    public string? Phone { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int City { get; set;  }
    public int Region { get; set; } 
    public string? INN { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal BonusBalance { get; set; }
    public Guid? CompanyId { get; set; }
    public decimal FiatBalance { get; set; }
    public string? VerificationCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEmailVerified { get; set; }
    public bool PincodeSet { get; set; } 
    public bool IsDeleted { get; set; }
    public string? DeviceToken { get; set; }
    public Guid? StoreId { get; set; }
}
public record PinCodeCreationDto
{
    public string UserId { get; set; } 
    public string PinCode { get; set; } 
}
public record BuyerRegistrationDto
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty; 
    public string DeviceToken { get; init; } = string.Empty;
}
public class BuyerUpdateDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Phone { get; init; } = string.Empty;
    public int City { get; init;  }
    public int Region { get; init; } 
    public string? INN { get; init; } = string.Empty;
}

public record UserRegistrationDto
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string? DeviceToken { get; set; }
}

public record SellerRegistrationDto
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public string? DeviceToken { get; set; } 
    public Guid StoreId { get; init; }
}
public record CompanySellerDto
{
    public Guid? UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? DeviceToken { get; set; }
    public Guid StoreId { get; init; }
}
public record UserLoginDto
{
    public string? Email { get; init; } = string.Empty;
    public string? DeviceToken { get; set; }
}
public record PinCodeLoginUser
{
    public Guid UserId { get; init; } 
    public string PinCode { get; init; }
}
public record AdminLoginDto
{
    public string? Email { get; init; }
}

// public class UserContextDto
// {
//     public Guid UserId { get; init; }
//     public string Username { get; init; } = string.Empty;
//     public UserRole Role { get; init; }
//     public string? Phone { get; init; }
//     public string FirstName { get; init; } = string.Empty;
//     public string LastName { get; init; } = string.Empty;
//     public string? Locality { get; init; } = string.Empty;
//     public string? Region { get; init; } = string.Empty; 
//     public string? INN { get; init; } = string.Empty;
//     public bool IsEmailVerified { get; set; }
//     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     public decimal BonusBalance { get; init; }
//     public Guid? CompanyId { get; init; }
// }

public record PermittedActionDto
{
    public string ActionName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
}

public record BonusBalanceDto
{
    public Guid UserId { get; init; } 
    public string FrontendId { get; init; } 
    public decimal BonusBalance { get; init; } 
    public decimal FiatBalance { get; init; }
} 

public class Result<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<T>? Data { get; set; }
}
using BonusSystem.Shared.Models;

namespace BonusSystem.Shared.Dtos;

/// <summary>
/// DTO for registering a company with its admin user
/// </summary>
public record CompanyWithAdminRegistrationDto
{
    // Company Information
    public string Name { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public decimal InitialBonusBalance { get; init; } 
    public decimal BonusBalance { get; init; } 
    public decimal FiatBalance { get; init; } 
    public decimal OriginalBonusBalance { get; init; } 
    public string BIK { get; init; } = string.Empty;
    public string BIN { get; set; } = string.Empty;
    public string INN { get; set; } = string.Empty;
    public int City { get; init; } 
    public int Region { get; init; } 
    // Company Admin Information
    public string AdminUsername { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
}


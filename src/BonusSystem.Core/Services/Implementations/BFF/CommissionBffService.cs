using BonusSystem.Core.Services.Interfaces; 

using Microsoft.Extensions.Configuration;

namespace BonusSystem.Core.Services.Implementations.BFF; 

public class CommissionBffService : ICommissionBffService
{
    private readonly IConfiguration _configuration;
    public CommissionBffService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public decimal GetDefaultCommissionPercent()
    {
        var percent = _configuration["Commission:Percent"];
        if (decimal.TryParse(percent, out var value))
            return value;
        return 5; // fallback default
    }
}


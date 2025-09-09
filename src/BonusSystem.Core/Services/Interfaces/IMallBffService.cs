using BonusSystem.Shared.Dtos; 

namespace BonusSystem.Core.Services.Interfaces;

public interface IMallBffService
{
    Task<PagedResult<MallDto>> GetMallsAsync(int page, int pagesize);
    Task<MallDto?> GetMallByIdAsync(Guid mallId);
    Task<MallDto?> GetMallByName(string name); 
} 

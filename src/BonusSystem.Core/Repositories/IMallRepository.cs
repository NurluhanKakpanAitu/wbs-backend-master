using BonusSystem.Shared.Dtos; 

namespace BonusSystem.Core.Repositories;

public interface IMallRepository : IRepository<MallDto, Guid>
{
    Task<MallDto?> GetByNameAsync(string name); 
    Task<IEnumerable<MallDto>> GetAllByLocationAsync(int City, int Region); 

}
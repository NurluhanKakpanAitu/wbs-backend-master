using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BonusSystem.Shared.Dtos;

namespace BonusSystem.Core.Repositories
{
    public interface ICategoryRepository
    {
        Task<int> GetCountAsync();
        Task<List<CategoryDto>> GetPagedAsync(int page, int pageSize);
        Task<CategoryDto?> GetByIdAsync(Guid id);
    }
}

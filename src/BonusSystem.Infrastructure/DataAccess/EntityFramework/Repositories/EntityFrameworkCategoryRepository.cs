using BonusSystem.Core.Repositories;
using BonusSystem.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkCategoryRepository : ICategoryRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkCategoryRepository> _logger;

    public EntityFrameworkCategoryRepository(BonusSystemContext dbContext, ILogger<EntityFrameworkCategoryRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> GetCountAsync()
    {
        try
        {
            return await _dbContext.Categories.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            throw;
        }
    }

    public async Task<List<CategoryDto>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            var categories = await _dbContext.Categories
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged categories");
            throw;
        }
        
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null) return null;
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category by ID");
            throw;
        }
        
    }
}


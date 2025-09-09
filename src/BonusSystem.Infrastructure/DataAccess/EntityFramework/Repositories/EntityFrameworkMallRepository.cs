using BonusSystem.Shared.Dtos;
using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BonusSystem.Core.Common.IDGenerator; 

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkMallRepository : IMallRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly IDGenerator _idGenerator;
    private readonly ILogger<EntityFrameworkMallRepository> _logger;

    public EntityFrameworkMallRepository(BonusSystemContext dbContext, ILogger<EntityFrameworkMallRepository> logger, IDGenerator idGenerator)
    {
        _dbContext = dbContext;
        _logger = logger; 
        _idGenerator = idGenerator;
    }

    public async Task<Guid> CreateAsync(MallDto body)
    {
        try
        { 
            var mall = new MallEntity
            {
                Id = body.Id,
                FrontendId = body.FrontendId,
                Name = body.Name,
                City = body.City,
                Region = body.Region,
                WorkHours = body.WorkHours
            };
            _dbContext.Malls.Add(mall);
            await _dbContext.SaveChangesAsync();
            return mall.Id;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    public async Task<bool> DeleteAsync(Guid id)
    { 
        try
        {
            var mall = await _dbContext.Malls.FindAsync(id);
            if (mall == null)
                throw new Exception("Mall not found");
            
            _dbContext.Malls.Remove(mall);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    public async Task<MallDto?> GetByIdAsync(Guid id)
    { 
        try
        {
            var mall = await _dbContext.Malls.FindAsync(id);
            if (mall == null)
                throw new Exception("Mall not found");
            
            return MapToDto(mall);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    } 
    public async Task<IEnumerable<MallDto>> GetAllAsync()
    {
        try
        {
            var entities = await _dbContext.Malls.AsNoTracking().ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }
    public async Task<bool> UpdateAsync(MallDto body)
    {
        try
        {
            var mall = await _dbContext.Malls.FindAsync(body.Id); 
            if (mall == null)
                throw new Exception("Mall not found");
            
            mall.Name = body.Name;
            mall.City = body.City;
            mall.Region = body.Region;
            mall.WorkHours = body.WorkHours;
            _dbContext.Malls.Update(mall);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        } 
    }
    public async Task<MallDto?> GetByNameAsync(string name)
    { 
        try
        {
            var mall = await _dbContext.Malls.AsNoTracking().FirstOrDefaultAsync(u => u.Name == name);
            if (mall == null)
                throw new Exception("Mall not found");
            
            return MapToDto(mall);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        } 
    }
    public async Task<IEnumerable<MallDto>> GetAllByLocationAsync(int City, int Region)
    {
        try
        { 
            var malls = await _dbContext.Malls.Where(m => m.City == City && m.Region == Region).ToListAsync();
            return malls.Select(MapToDto);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        } 
    } 

    private MallDto MapToDto(MallEntity entity)
    {
        return new MallDto
        {
            Id = entity.Id,
            FrontendId = entity.FrontendId,
            Name = entity.Name,
            City = entity.City,
            Region = entity.Region,
            WorkHours = entity.WorkHours
        };
    }
    private MallEntity MapToEntity(MallDto entity)
    {
        return new MallEntity
        {
            Id = entity.Id,
            FrontendId = entity.FrontendId,
            Name = entity.Name,
            City = entity.City,
            Region = entity.Region,
            WorkHours = entity.WorkHours
        }; 
    }
}
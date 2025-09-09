using BonusSystem.Shared.Dtos;
using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.EntityFramework;
using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore; 

public class EntityFrameworkFiatReplenishmentCompanyBalanceRepository : IFiatReplenishmentCompanyBalanceRepository
{
    private readonly BonusSystemContext _dbContext;

    public EntityFrameworkFiatReplenishmentCompanyBalanceRepository(BonusSystemContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateAsync(FiatReplenishmentCompanyBalanceDto body)
    {
        var entity = new FiatReplenishmentCompanyBalanceEntity
        {
            Id = Guid.NewGuid(),
            CompanyId = body.CompanyId,
            Amount = body.Amount,
            Timestamp = DateTime.UtcNow
        };

        await _dbContext.FiatReplenishmentCompanyBalances.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return entity.Id;
    }
    public async Task<IEnumerable<FiatReplenishmentCompanyBalanceDto>> GetAllAsync()
    {
        var entities = await _dbContext.FiatReplenishmentCompanyBalances.AsNoTracking().ToArrayAsync();
        return entities.Select(MapToDto).ToList(); 
    }
    public async Task<FiatReplenishmentCompanyBalanceDto?> GetByIdAsync(Guid id)
    {
        var entity = await _dbContext.FiatReplenishmentCompanyBalances.FindAsync(id);
        if (entity == null) return null;
        return MapToDto(entity); 
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.FiatReplenishmentCompanyBalances.FindAsync(id);
        if (entity == null) return false;
        _dbContext.FiatReplenishmentCompanyBalances.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true; 
    }
    public async Task<bool> UpdateAsync(FiatReplenishmentCompanyBalanceDto body)
    {
        var entity = await _dbContext.FiatReplenishmentCompanyBalances.FindAsync(body.Id);
        if (entity == null) return false;
        entity.CompanyId = body.CompanyId;
        entity.Amount = body.Amount;
        entity.Timestamp = body.Timestamp;
        return true; 
    }
    private FiatReplenishmentCompanyBalanceEntity MapToEntity(FiatReplenishmentCompanyBalanceDto dto)
    {
        return new FiatReplenishmentCompanyBalanceEntity
        {
            Id = dto.Id,
            CompanyId = dto.CompanyId,
            Amount = dto.Amount,
            Timestamp = dto.Timestamp
        };
    } 
    private FiatReplenishmentCompanyBalanceDto MapToDto(FiatReplenishmentCompanyBalanceEntity dto)
    { 
        return new FiatReplenishmentCompanyBalanceDto
        {
            Id = dto.Id,
            CompanyId = dto.CompanyId,
            Amount = dto.Amount,
            Timestamp = dto.Timestamp
        };
    }
}
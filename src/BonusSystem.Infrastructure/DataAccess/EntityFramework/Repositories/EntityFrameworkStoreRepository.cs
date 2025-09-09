using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BonusSystem.Core.Common.IDGenerator; 

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkStoreRepository : IStoreRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkStoreRepository> _logger;
    private readonly IIDGenerator _idgen;

    public EntityFrameworkStoreRepository(BonusSystemContext dbContext, IIDGenerator idGenerator,ILogger<EntityFrameworkStoreRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _idgen = idGenerator;
    }

    public async Task<IEnumerable<StoreDto>> GetAllAsync()
    {
        try
        {
            var entities = await _dbContext.Stores.AsNoTracking().ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all stores");
            throw;
        }
    }

    public async Task<StoreDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.Stores.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving store with ID {Id}", id);
            throw;
        }
    }

    public async Task<Guid> CreateAsync(StoreDto dto)
    {
        try
        {
            var entity = MapToEntity(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _dbContext.Stores.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating store {Name}", dto.Name);
            throw;
        }
    }
    public async Task<int> GetCountAsync(int categoryId)
    {
        var query = _dbContext.Stores.AsNoTracking();
        // CategoryId filtering removed because StoreEntity does not have CategoryId
        return await query.Select(s => s.CompanyId).Distinct().CountAsync();
    }

    public async Task<List<StoreDto>> GetPagedAsync(int categoryId, int page, int pageSize)
    {
        var query = _dbContext.Stores.AsNoTracking();
        // CategoryId filtering removed because StoreEntity does not have CategoryId

        // Get the company ID for the current page.
        var companyId = await query
            .Select(s => s.CompanyId)
            .Distinct()
            .OrderBy(id => id) // Need a consistent order for pagination
            .Skip(page - 1)
            .FirstOrDefaultAsync();

        if (companyId == Guid.Empty)
        {
            return new List<StoreDto>();
        }

        // Get stores for that company. The result size will be at most pageSize.
        var entities = await query
            .Where(s => s.CompanyId == companyId)
            .OrderBy(s => s.Name)
            .Take(pageSize)
            .ToListAsync();
            
        return entities.Select(MapToDto).ToList();
    }
    public async Task<bool> UpdateAsync(StoreDto dto)
    {
        try
        {
            var entity = await _dbContext.Stores.FindAsync(dto.Id);
            if (entity == null)
            {
                return false;
            }

            entity.UserName = dto.UserName;
            entity.PasswordHash = dto.PasswordHash;
            entity.CompanyId = dto.CompanyId;
            entity.Name = dto.Name;
            entity.MallId = dto.MallId;
            entity.Floor = dto.Floor;
            entity.Row = dto.Row;
            entity.Number = dto.Number;
            entity.Email = dto.Email;
            entity.WorkingHours = dto.WorkingHours;
            entity.City = dto.City;
            entity.Region = dto.Region;
            entity.Address = dto.Address;
            entity.BusinessTypeId = dto.BusinessTypeId;
            entity.CategoryId = dto.CategoryId;
            entity.ContactPhone = dto.ContactPhone;
            entity.Status = dto.Status;
            
            entity.UpdatedAt = DateTime.UtcNow;
            _dbContext.Stores.Update(entity);

            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating store with ID {Id}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.Stores.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _dbContext.Stores.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting store with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<StoreDto>> GetStoresByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var entities = await _dbContext.Stores.AsNoTracking()
                .Where(s => s.CompanyId == companyId)
                .ToListAsync();

            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stores for company ID {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<int> GetStoresCountByCompanyIdAsync(Guid companyId)
    {
        return await _dbContext.Stores.AsNoTracking()
            .CountAsync(s => s.CompanyId == companyId);
    }

    public async Task<PagedResult<StoreDto>> GetStoresByCompanyIdPagedAsync(Guid companyId, int page, int pageSize)
    {
        var query = _dbContext.Stores.AsNoTracking()
            .Where(s => s.CompanyId == companyId);

        var totalCount = await query.CountAsync();

        var entities = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var dtos = entities.Select(MapToDto).ToList();

        return new PagedResult<StoreDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateStatusAsync(Guid storeId, StoreStatus status)
    {
        var entity = await _dbContext.Stores.FirstOrDefaultAsync(s => s.Id == storeId);
        if (entity == null)
        {
            return false;
        }
        entity.Status = status;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveSellerFromStoreAsync(Guid storeId, Guid sellerId)
    {
        try
        {
            var affected = await _dbContext.Users
                .Where(u => u.Id == sellerId && u.Role == UserRole.Seller && u.StoreId == storeId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.StoreId, (Guid?)null));

            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing seller {SellerId} from store {StoreId}", sellerId, storeId);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetSellersByStoreIdAsync(Guid storeId)
    {
        try
        {
            var sellers = await _dbContext.Stores
                .AsNoTracking()
                .Where(s => s.Id == storeId)
                .SelectMany(s => s.Sellers.Where(u => u.Role == UserRole.Seller))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FrontendId = u.FrontendId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    BonusBalance = u.BonusBalance
                })
                .ToListAsync();

            return sellers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sellers for store ID {StoreId}", storeId);
            throw;
        }
    }

    public async Task<IEnumerable<StoreDto>> GetStoresByCategoryAsync(string category)
    {
        try
        {
            var allStores = await _dbContext.Stores.AsNoTracking()
                .Where(s => s.Status == StoreStatus.Active)
                .ToListAsync();

            return allStores.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stores by category {Category}", category);
            throw;
        }
    }

    public async Task<StoreDto?> GetStoreBySellerIdAsync(Guid sellerId)
    {
        try
        {
            var store = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == sellerId && u.Role == UserRole.Seller)
                .Select(u => u.Store)
                .FirstOrDefaultAsync();

            return store is null ? null : MapToDto(store);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving store for seller {SellerId}", sellerId);
            throw;
        }
    }

    public async Task<decimal> GetStoreBonusBalanceAsync(Guid storeId)
    {
        try
        {
            // This is a bit of a complex calculation - in a real implementation,
            // you might have a different approach, but for the prototype:
            // Sum all completed transactions for this store
            var transactionsSum = await _dbContext.BonusTransactions
                .Where(t => t.StoreId == storeId && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.Type == TransactionType.Earn ? t.BonusAmount : -t.BonusAmount);

            return transactionsSum;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating bonus balance for store with ID {Id}", storeId);
            throw;
        }
    }
    public async Task<decimal> GetStoreFiatBalanceAsync(Guid storeId)
    {
        try
        {
            // This is a bit of a complex calculation - in a real implementation,
            // you might have a different approach, but for the prototype:
            // Sum all completed fiat transactions for this store
            var store_company_id = await _dbContext.Stores
                .Where(s => s.Id == storeId)
                .Select(s => s.CompanyId)
                .FirstOrDefaultAsync();

            var fiat_balance_store = await _dbContext.Companies
                .Where(c => c.Id == store_company_id)
                .Select(c => c.FiatBalance)
                .FirstOrDefaultAsync();

            return fiat_balance_store;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fiat balance for store with ID {Id}", storeId);
            throw;
        }
    }
    public async Task<PagedResult<CombinedTransactionDto>> AllTransactionsForStoreAsync(Guid storeId, int page, int pageSize)
    {
        try
        {   
            var bonusTransactionsQuery =
                from t in _dbContext.BonusTransactions
                join u in _dbContext.Users on t.UserId equals u.Id
                where t.StoreId == storeId
                select new CombinedTransactionDto
                {
                    Id = t.Id,
                    Timestamp = t.Timestamp,
                    Amount = 0, // No fiat amount for a bonus-only transaction
                    BonusAmount = t.BonusAmount,
                    CashbackAmount = 0, // No cashback for a bonus-only transaction
                    Type = "Bonus",
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    ClientFrontendId = u.FrontendId,
                    ClientName = u.Username
                };

            var fiatTransactionsQuery =
                from t in _dbContext.FiatTransactions
                join u in _dbContext.Users on t.UserId equals u.Id
                where t.StoreId == storeId
                select new CombinedTransactionDto
                {
                    Id = t.Id,
                    Timestamp = t.Timestamp,
                    Amount = t.FiatTransactionAmount,
                    BonusAmount = t.BonusAmount,
                    CashbackAmount = t.FiatCashBackAmount,
                    Type = "Fiat",
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    ClientFrontendId = u.FrontendId,
                    ClientName = u.Username
                };

            var combinedQuery = bonusTransactionsQuery.Union(fiatTransactionsQuery);

            var totalCount = await combinedQuery.CountAsync();

            var combinedTransactions = await combinedQuery
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CombinedTransactionDto>
            {
                Items = combinedTransactions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all transactions for store {StoreId}", storeId);
            throw;
        }
    }
    private StoreDto MapToDto(StoreEntity entity)
    {
        return new StoreDto
        {
            Id = entity.Id,
            FrontendId = entity.FrontendId,
            UserName = entity.UserName,
            BusinessTypeId = entity.BusinessTypeId,
            PasswordHash = entity.PasswordHash,
            CompanyId = entity.CompanyId,
            Name = entity.Name,
            MallId = entity.MallId,
            Floor = entity.Floor,
            Row = entity.Row,
            Number = entity.Number,
            Email = entity.Email,
            WorkingHours = entity.WorkingHours,
            City = entity.City,
            Region = entity.Region,
            Address = entity.Address,
            CategoryId = entity.CategoryId,
            TypeOfbusiness = entity.TypeOfbusiness,
            ContactPhone = entity.ContactPhone,
            Status = entity.Status
        };
    }

    private StoreEntity MapToEntity(StoreDto dto)
    {
        return new StoreEntity
        {
            Id = dto.Id,
            FrontendId = dto.FrontendId,
            UserName = dto.UserName,
            PasswordHash = dto.PasswordHash,
            CompanyId = dto.CompanyId,
            Name = dto.Name,
            ContactPhone = dto.ContactPhone,
            MallId = dto.MallId,
            BusinessTypeId = dto.BusinessTypeId,
            Floor = dto.Floor,
            Row = dto.Row,
            Number = dto.Number,
            City = dto.City,
            Region = dto.Region,
            Address = dto.Address,
            Email = dto.Email,
            WorkingHours = dto.WorkingHours,
            CategoryId = dto.CategoryId,
            TypeOfbusiness = dto.TypeOfbusiness,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BonusSystem.Core.Common.IDGenerator;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkFiatTransactionRepository : IFiatTransactionRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkFiatTransactionRepository> _logger;
    private readonly FiatTransactionOptions _options;
    private readonly IIDGenerator _idGenerator;

    public EntityFrameworkFiatTransactionRepository(BonusSystemContext dbContext, ILogger<EntityFrameworkFiatTransactionRepository> logger, IOptions<FiatTransactionOptions> options, IIDGenerator idGenerator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _options = options.Value;
        _idGenerator = idGenerator;
    }

    public async Task<IEnumerable<FiatTransactionDto>> GetAllAsync()
    {
        try
        {
            var entities = await _dbContext.FiatTransactions.AsNoTracking().ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all FiatTransactions");
            throw;
        }
    }

    public async Task<FiatTransactionDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.FiatTransactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fiat transaction with ID {Id}", id);
            throw;
        }
    }

    public async Task<Guid> CreateAsync(FiatTransactionDto dto)
    {
        try
        {
            var entity = MapToEntity(dto);
            entity.Timestamp = DateTime.UtcNow;
            
            await _dbContext.FiatTransactions.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fiat transaction");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(FiatTransactionDto dto)
    {
        try
        {
            var entity = await _dbContext.FiatTransactions.FindAsync(dto.Id);
            if (entity == null)
            {
                return false;
            }

            entity.TotalCost = dto.TotalCost;
            entity.BonusAmount = dto.BonusAmount;
            entity.FiatTransactionAmount = dto.FiatTransactionAmount;
            entity.FiatCashBackAmount = dto.FiatCashBackAmount;
            entity.FiatCashBackRate = dto.FiatCashBackRate;
            entity.Status = dto.Status;
            entity.Description = dto.Description;
            
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fiat transaction with ID {Id}", dto.Id);
            throw;
        }
    }
    public IQueryable<FiatTransactionDto> GetTransactionsByUserIdQuery(Guid userId)
    {
        return _dbContext.FiatTransactions
            .Where(t => t.UserId == userId)
            .Select(t => new FiatTransactionDto
            {
                Id = t.Id,
                FrontendId = t.FrontendId,
                UserId = t.UserId,
                CompanyId = t.CompanyId,
                StoreId = t.StoreId,
                TotalCost = t.TotalCost,
                BonusAmount = t.BonusAmount,
                Timestamp = t.Timestamp,
                Status = t.Status,
                Description = t.Description,
                FiatCashBackAmount = t.FiatCashBackAmount,
                FiatCashBackRate = t.FiatCashBackRate,
                FiatTransactionAmount = t.FiatTransactionAmount,
                SellerId = t.SellerId,
                CommissionPercent = t.CommissionPercent
            });
    }
    public async Task<IEnumerable<FiatTransactionDto>> GetTransactionsByUserIdAsync(Guid userId)
    {
        try
        {
            var entities = await _dbContext.FiatTransactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fiat transactions for user ID {UserId}", userId);
            throw;
        }
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.FiatTransactions.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _dbContext.FiatTransactions.Remove(entity);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Fiat transaction with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsByUserIdAsync(Guid userId)
    {
        try
        {
            var entities = await _dbContext.FiatTransactions.AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fiat transactions for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var entities = await _dbContext.FiatTransactions.AsNoTracking()
                .Where(t => t.CompanyId == companyId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fiat transactions for company ID {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsByStoreIdAsync(Guid storeId)
    {
        try
        {
            var entities = await _dbContext.FiatTransactions.AsNoTracking()
                .Where(t => t.StoreId == storeId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fiat transactions for store ID {StoreId}", storeId);
            throw;
        }
    }

    public async Task<bool> UpdateFiatTransactionStatusAsync(Guid transactionId, FiatTransactionStatus status)
    {
        try
        {
            var entity = await _dbContext.FiatTransactions.FindAsync(transactionId);
            if (entity == null)
            {
                return false;
            }

            entity.Status = status;
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for fiat transaction with ID {Id}", transactionId);
            throw;
        }
    }

    public async Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionsInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var entities = await _dbContext.FiatTransactions.AsNoTracking()
                .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fiat transactions in date range {StartDate} to {EndDate}", 
                startDate, endDate);
            throw;
        }
    }


    public async Task<int> GetTotalFiatTransactionsCountAsync()
    {
        try
        {
            return await _dbContext.FiatTransactions.AsNoTracking().CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting total fiat transactions");
            throw;
        }
    }

    public async Task<IEnumerable<FiatTransactionDto>> GetActiveFiatTransactionsForUserAsync(Guid userId)
    {
        try
        {
            // Get all completed transactions for this user
            var entities = await _dbContext.FiatTransactions.AsNoTracking()
                .Where(t => t.UserId == userId && t.Status == FiatTransactionStatus.Completed)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active fiat transactions for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<FiatTransactionDto>> GetActiveFiatTransactionsForCompanyAsync(Guid companyId)
    {
        try
        {
            // Get all completed FiatTransactions for this company
            var entities = await _dbContext.FiatTransactions.AsNoTracking()
                .Where(t => t.CompanyId == companyId && t.Status == FiatTransactionStatus.Completed)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active fiat transactions for company ID {CompanyId}", companyId);
            throw;
        }
    }
    public async Task<IEnumerable<FiatTransactionDto>> GetPendingFiatTransactionsAsync(FiatTransactionStatus status)
    {
        try
        {
            var thresholdTime = DateTime.UtcNow.AddMinutes(-_options.PendingTresholdMinutes);

            var entities = await _dbContext.FiatTransactions.AsNoTracking()
            .Where(t => t.Status == status && t.Timestamp <= thresholdTime)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving FiatTransactions by status");
            throw;
        }
    }

    public async Task CreatePendingTransactionAsync(FiatTransactionDto transaction)
    {
        try
        {
            var entity = MapToEntity(transaction);
            entity.Status = FiatTransactionStatus.Pending;
            entity.Timestamp = DateTime.UtcNow;
            await _dbContext.FiatTransactions.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pending FiatTransaction");
            throw;
        }
    }

    private FiatTransactionDto MapToDto(FiatTransactionEntity entity)
    {
        return new FiatTransactionDto
        {
            Id = entity.Id,
            FrontendId = entity.FrontendId,
            UserId = entity.UserId,
            CompanyId = entity.CompanyId,
            StoreId = entity.StoreId,
            TotalCost = entity.TotalCost,
            BonusAmount = entity.BonusAmount,
            Timestamp = entity.Timestamp,
            Status = entity.Status,
            Description = entity.Description,
            FiatCashBackAmount = entity.FiatCashBackAmount,
            FiatCashBackRate = entity.FiatCashBackRate,
            FiatTransactionAmount = entity.FiatTransactionAmount,
            SellerId = entity.SellerId,
            CommissionPercent = entity.CommissionPercent,

        };
    }

    private FiatTransactionEntity MapToEntity(FiatTransactionDto dto)
    {
        return new FiatTransactionEntity()
        {
            Id = dto.Id,
            FrontendId = string.IsNullOrEmpty(dto.FrontendId) ? _idGenerator.NewId() : dto.FrontendId,
            UserId = dto.UserId,
            CompanyId = dto.CompanyId,
            StoreId = dto.StoreId,
            TotalCost = dto.TotalCost,
            BonusAmount = dto.BonusAmount,
            Timestamp = dto.Timestamp,
            Status = dto.Status,
            Description = dto.Description,
            FiatCashBackAmount = dto.FiatCashBackAmount,
            FiatCashBackRate = dto.FiatCashBackRate,
            FiatTransactionAmount = dto.FiatTransactionAmount,
            SellerId = dto.SellerId,
            CommissionPercent = dto.CommissionPercent,
            RelatedBonusTransactionId = dto.RelatedBonusTransactionId,
        };
    }
}
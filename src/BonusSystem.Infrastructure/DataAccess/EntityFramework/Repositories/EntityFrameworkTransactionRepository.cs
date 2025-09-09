using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkTransactionRepository : ITransactionRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkTransactionRepository> _logger;

    public EntityFrameworkTransactionRepository(BonusSystemContext dbContext, ILogger<EntityFrameworkTransactionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync()
    {
        try
        {
            var entities = await _dbContext.BonusTransactions.AsNoTracking().ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all BonusTransactions");
            throw;
        }
    }
    public IQueryable<TransactionDto> GetTransactionsByUserIdQuery(Guid userId)
    {
        return _dbContext.BonusTransactions
            .Where(t => t.UserId == userId)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                CompanyId = t.CompanyId,
                StoreId = t.StoreId,
                TotalCost = t.TotalCost,
                BonusAmount = t.BonusAmount,
                Type = t.Type,
                Timestamp = t.Timestamp,
                Status = t.Status,
                Description = t.Description,
                CommissionPercent = t.CommissionPercent
            });
    }
    public async Task<TransactionDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.BonusTransactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction with ID {Id}", id);
            throw;
        }
    }

    public async Task<Guid> CreateAsync(TransactionDto dto)
    {
        try
        {
            var entity = MapToEntity(dto);
            entity.Timestamp = DateTime.UtcNow;
            
            await _dbContext.BonusTransactions.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for {Type}", dto.Type);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(TransactionDto dto)
    {
        try
        {
            var entity = await _dbContext.BonusTransactions.FindAsync(dto.Id);
            if (entity == null)
            {
                return false;
            }

            entity.TotalCost = dto.TotalCost;
            entity.BonusAmount = dto.BonusAmount;
            entity.Type = dto.Type;
            entity.Status = dto.Status;
            entity.Description = dto.Description;
            
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction with ID {Id}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.BonusTransactions.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _dbContext.BonusTransactions.Remove(entity);
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByUserIdAsync(Guid userId)
    {
        try
        {
            var entities = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var entities = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.CompanyId == companyId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for company ID {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByStoreIdAsync(Guid storeId)
    {
        try
        {
            var entities = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.StoreId == storeId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for store ID {StoreId}", storeId);
            throw;
        }
    }

    public async Task<CommissionProcessingResult> ProcessCompanyCommissionsAsync()
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var commissionsByCompany = await _dbContext.BonusTransactions
                    .Where(t => t.Type == TransactionType.Earn &&
                                t.Status == TransactionStatus.Completed &&
                                t.CompanyId.HasValue &&
                                !t.IsCommissionProcessed)
                    .GroupBy(t => t.CompanyId!.Value)
                    .Select(g => new
                    {
                        CompanyId = g.Key,
                        TotalCommission = g.Sum(t => t.BonusAmount * (t.CommissionPercent / 100.0m)),
                        TransactionsCount = g.Count()
                    })
                    .ToListAsync();

                if (commissionsByCompany.Count == 0)
                {
                    await dbTransaction.CommitAsync();
                    return new CommissionProcessingResult(0, 0, 0m);
                }

                var companyIds = commissionsByCompany.Select(c => c.CompanyId).ToList();
                var companiesToUpdate = await _dbContext.Companies
                    .Where(c => companyIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id);

                decimal totalCommission = 0m;
                int updatedCompanies = 0;
                int processedTransactions = commissionsByCompany.Sum(c => c.TransactionsCount);

                foreach (var commissionInfo in commissionsByCompany)
                {
                    if (companiesToUpdate.TryGetValue(commissionInfo.CompanyId, out var company))
                    {
                        company.BonusBalance -= commissionInfo.TotalCommission;
                        totalCommission += commissionInfo.TotalCommission;
                        updatedCompanies++;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Company with ID {CompanyId} not found for commission processing, but transactions exist.",
                            commissionInfo.CompanyId);
                    }
                }

                await _dbContext.BonusTransactions
                    .Where(t => t.Type == TransactionType.Earn &&
                                t.Status == TransactionStatus.Completed &&
                                t.CompanyId.HasValue &&
                                !t.IsCommissionProcessed)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsCommissionProcessed, true));

                await _dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new CommissionProcessingResult(processedTransactions, updatedCompanies, totalCommission);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Error processing company commissions.");
                throw;
            }
        });
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByTypeAsync(TransactionType type)
    {
        try
        {
            var entities = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions of type {Type}", type);
            throw;
        }
    }

    public async Task<bool> UpdateTransactionStatusAsync(Guid transactionId, TransactionStatus status)
    {
        try
        {
            var entity = await _dbContext.BonusTransactions.FindAsync(transactionId);
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
            _logger.LogError(ex, "Error updating status for transaction with ID {Id}", transactionId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsInDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var entities = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions in date range {StartDate} to {EndDate}", 
                startDate, endDate);
            throw;
        }
    }

    public async Task<decimal> GetTotalBonusCirculationAsync()
    {
        try
        {
            // Sum of all Earn transactions
            var totalEarned = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.Type == TransactionType.Earn && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.BonusAmount);
            
            // Sum of all Spend transactions
            var totalSpent = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.Type == TransactionType.Spend && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.BonusAmount);
            
            // Total circulation is Earned + Spent
            return totalEarned + totalSpent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total bonus circulation");
            throw;
        }
    }

    public async Task<decimal> GetTotalActiveBonus()
    {
        try
        {
            // Sum of all user balances 
            var totalActiveBonus = await _dbContext.Users.AsNoTracking()
                .SumAsync(u => u.BonusBalance);
            
            return totalActiveBonus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total active bonus");
            throw;
        }
    }

    public async Task<int> GetTotalTransactionsCountAsync()
    {
        try
        {
            return await _dbContext.BonusTransactions.AsNoTracking().CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting total transactions");
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetActiveTransactionsForUserAsync(Guid userId)
    {
        try
        {
            // Get all completed transactions for this user
            var entities = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.UserId == userId && t.Status == TransactionStatus.Completed)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active transactions for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionDto>> GetActiveTransactionsForCompanyAsync(Guid companyId)
    {
        try
        {
            // Get all completed BonusTransactions for this company
            var entities = await _dbContext.BonusTransactions.AsNoTracking()
                .Where(t => t.CompanyId == companyId && t.Status == TransactionStatus.Completed)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active transactions for company ID {CompanyId}", companyId);
            throw;
        }
    }

    public async Task ProcessExpirationAsync(DateTime expirationDate)
    {
        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Get all users with positive bonus balance
                    var usersWithBonus = await _dbContext.Users
                        .Where(u => u.BonusBalance > 0)
                        .ToListAsync();
                    
                    foreach (var user in usersWithBonus)
                    {
                        // Create an expiration transaction
                        var expirationTransaction = new BonusTransactionEntity
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            BonusAmount = user.BonusBalance,
                            Type = TransactionType.Expire,
                            Status = TransactionStatus.Completed,
                            Timestamp = expirationDate,
                            Description = "Quarterly bonus expiration"
                        };
                        
                        // Add the expiration transaction
                        await _dbContext.BonusTransactions.AddAsync(expirationTransaction);
                        
                        // Reset user's balance to 0
                        user.BonusBalance = 0;
                    }
                    
                    // Get all companies
                    var companies = await _dbContext.Companies.ToListAsync();
                    
                    foreach (var company in companies)
                    {
                        // Reset company balance to original
                        company.BonusBalance = company.OriginalBonusBalance;
                    }
                    
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bonus expiration for date {ExpirationDate}", expirationDate);
            throw;
        }
    }

    public async Task<IReadOnlyList<TransactionDto>> GetPendingAsync()
    {
        var entities = await _dbContext.BonusTransactions
            .Where(t => t.Status == TransactionStatus.Pending)
            .ToListAsync();
        return entities.Select(MapToDto).ToList();
    }

    private TransactionDto MapToDto(BonusTransactionEntity entity)
    {
        return new TransactionDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            CompanyId = entity.CompanyId,
            StoreId = entity.StoreId,
            TotalCost = entity.TotalCost,
            BonusAmount = entity.BonusAmount,
            Type = entity.Type,
            Timestamp = entity.Timestamp,
            Status = entity.Status,
            Description = entity.Description,
            CommissionPercent = entity.CommissionPercent,
            IsCommissionProcessed = entity.IsCommissionProcessed
        };
    }

    private BonusTransactionEntity MapToEntity(TransactionDto dto)
    {
        return new BonusTransactionEntity()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            CompanyId = dto.CompanyId,
            StoreId = dto.StoreId,
            TotalCost = dto.TotalCost,
            BonusAmount = dto.BonusAmount,
            Type = dto.Type,
            Timestamp = dto.Timestamp,
            Status = dto.Status,
            Description = dto.Description,
            CommissionPercent = dto.CommissionPercent,
            IsCommissionProcessed = dto.IsCommissionProcessed,
            SellerId = dto.SellerId
        };
    }
}
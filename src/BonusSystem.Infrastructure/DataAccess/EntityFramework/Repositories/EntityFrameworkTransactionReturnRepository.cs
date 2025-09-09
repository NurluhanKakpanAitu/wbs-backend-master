using System.Transactions;
using BonusSystem.Core.Repositories;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkTransactionReturnRepository : ITransactionReturnRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkTransactionReturnRepository> _logger;

    public EntityFrameworkTransactionReturnRepository(
        BonusSystemContext dbContext,
        ILogger<EntityFrameworkTransactionReturnRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TransactionReturnDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.TransactionReturns
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TransactionReturn with ID {Id}", id);
            throw;
        }
    }

    public async Task<Guid> CreateAsync(TransactionReturnDto dto)
    {
        try
        {
            var entity = MapToEntity(dto);
            await _dbContext.TransactionReturns.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TransactionReturn");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(TransactionReturnDto dto)
{
    try
    {
        var entity = await _dbContext.TransactionReturns.FindAsync(dto.Id);
        if (entity == null)
            return false;

        var updated = MapToEntity(dto);

        entity.Status = updated.Status;
        entity.ApprovedByUserId = updated.ApprovedByUserId;
        entity.ApprovedAt = updated.ApprovedAt;

        await _dbContext.SaveChangesAsync();
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating TransactionReturn with ID {Id}", dto.Id);
        throw;
    }
}

    public async Task<IEnumerable<TransactionReturnDto>> GetByStatusAsync(TransactionReturnStatus status)
    {
        try
        {
            var entities = await _dbContext.TransactionReturns
                .AsNoTracking()
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TransactionReturns with status {Status}", status);
            throw;
        }
    }
    public async Task<IEnumerable<TransactionReturnDto>> GetByRequestedUserIdAsync(Guid userId, TransactionReturnStatus? status = null)
    {
        try
        {
            var query = _dbContext.TransactionReturns
                .AsNoTracking()
                .Where(t => t.RequestedByUserId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var entities = await query
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();

            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction returns by requested user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionReturnDto>> GetByApprovedUserIdAsync(Guid userId)
    {
        try
        {
            var entities = await _dbContext.TransactionReturns
                .AsNoTracking()
                .Where(t => t.Status == TransactionReturnStatus.Approved && t.ApprovedByUserId == userId)
                .OrderByDescending(t => t.ApprovedAt)
                .ToListAsync();

            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction returns by approved user ID {UserId}", userId);
            throw;
        }
    }

public async Task<TransactionReturnDto?> GetByBonusTransactionIdAsync(Guid transactionId, TransactionReturnStatus? status = null)
{
    try
    {
        var query = _dbContext.TransactionReturns
            .AsNoTracking()
            .Where(t => t.BonusTransactionId == transactionId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var entity = await query.OrderByDescending(t => t.RequestedAt).FirstOrDefaultAsync();
        return entity != null ? MapToDto(entity) : null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving transaction returns by bonus transaction ID {TransactionId}", transactionId);
        throw;
    }
}

    public async Task<TransactionReturnDto?> GetByFiatTransactionIdAsync(Guid transactionId, TransactionReturnStatus? status = null)
    {
        try
        {
            var query = _dbContext.TransactionReturns
                .AsNoTracking()
                .Where(t => t.FiatTransactionId == transactionId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var entity = await query.OrderByDescending(t => t.RequestedAt).FirstOrDefaultAsync();

            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction returns by fiat transaction ID {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<IEnumerable<TransactionReturnDto>> GetPendingReturnsByBuyerIdAsync(Guid buyerId)
    {
        try
        {
            var entities = await _dbContext.TransactionReturns
                .AsNoTracking()
                .Where(tr => tr.Status == TransactionReturnStatus.Pending &&
                    (
                        (tr.BonusTransaction != null && tr.BonusTransaction.UserId == buyerId) ||
                        (tr.FiatTransaction != null && tr.FiatTransaction.UserId == buyerId)
                    ))
                .OrderByDescending(tr => tr.RequestedAt)
                .ToListAsync();

            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending transaction returns for buyer ID {BuyerId}", buyerId);
            throw;
        }
    }


    private TransactionReturnDto MapToDto(TransactionReturn entity)
    {
        return new TransactionReturnDto
        {
            Id = entity.Id,
            Reason = entity.Reason,
            Status = entity.Status,
            RequestedAt = entity.RequestedAt,
            RequestedByUserId = entity.RequestedByUserId,
            ApprovedByUserId = entity.ApprovedByUserId,
            BonusTransactionId = entity.BonusTransactionId,
            FiatTransactionId = entity.FiatTransactionId,
            ApprovedAt = entity.ApprovedAt,
            
        };
    }

    private TransactionReturn MapToEntity(TransactionReturnDto dto)
    {
        return new TransactionReturn
        {
            Id = dto.Id,
            Reason = dto.Reason,
            Status = dto.Status,
            RequestedAt = dto.RequestedAt,
            RequestedByUserId = dto.RequestedByUserId,
            ApprovedByUserId = dto.ApprovedByUserId,
            BonusTransactionId = dto.BonusTransactionId,
            FiatTransactionId = dto.FiatTransactionId,
            ApprovedAt = dto.ApprovedAt,
        };
    }
}

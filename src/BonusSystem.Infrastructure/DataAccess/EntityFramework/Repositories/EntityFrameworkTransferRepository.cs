
using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities; 
using BonusSystem.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkTransferRepository : ITransferRepository
{
    private readonly BonusSystemContext _context;
    private readonly ILogger<EntityFrameworkTransferRepository> _logger;
    public EntityFrameworkTransferRepository(BonusSystemContext context, ILogger<EntityFrameworkTransferRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TransferDto> GetByIdAsync(Guid id)
    {
        var transfer = await _context.Transfers
            .Include(t => t.Sender)
            .Include(t => t.Recipient)
            .FirstOrDefaultAsync(t => t.Id == id);
        return transfer != null ? MapToDto(transfer) : null;
    }

    public async Task<Guid> CreateAsync(TransferDto transfer)
    { 
        var entity = MapToEntity(transfer);
        _context.Transfers.Add(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }
    public async Task<bool> UpdateAsync(TransferDto transfer)
    {

        var entity = MapToEntity(transfer);
        _context.Transfers.Update(entity);
        await _context.SaveChangesAsync();
        return true;
        
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Transfers.FindAsync(id);
        if (entity == null)
            return false;
        _context.Transfers.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<IEnumerable<TransferDto>> GetAllAsync()
    {
        var transfers = await _context.Transfers
            .Include(t => t.Sender)
            .Include(t => t.Recipient)
            .ToListAsync();
        return transfers.Select(MapToDto).ToList();
    }
    private TransferEntity MapToEntity(TransferDto dto)
    {
        return new TransferEntity()
        {
            Id = dto.Id,
            SenderId = dto.SenderId,
            RecipientId = dto.RecipientId,
            Amount = dto.Amount,
            Timestamp = dto.Timestamp,
            Status = dto.Status,
            CommissionPercent = dto.CommissionPercent,
            Type = dto.Type
        };
    } 
    
    private TransferDto MapToDto(TransferEntity entity)
    {
        return new TransferDto
        {
            Id = entity.Id,
            SenderId = entity.SenderId,
            RecipientId = entity.RecipientId,
            Amount = entity.Amount,
            Timestamp = entity.Timestamp,
            Status = entity.Status,
            CommissionPercent = entity.CommissionPercent,
            Type = entity.Type
        };
    }
}
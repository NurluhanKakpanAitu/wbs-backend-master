using BonusSystem.Core.Exceptions;
using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkCompanyRepository : ICompanyRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkCompanyRepository> _logger;

    public EntityFrameworkCompanyRepository(BonusSystemContext dbContext,
        ILogger<EntityFrameworkCompanyRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        try
        {
            var entities = await _dbContext.Companies.AsNoTracking().ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all companies");
            throw;
        }
    }
    public async Task<CompanyDto?> GetBySellerId(Guid userId)
    {
        var company = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId )
            .Select(u => u.Store!.Company)
            .FirstOrDefaultAsync();

        return company is null ? null : MapToDto(company);
    }
    public async Task<CompanyDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.Companies.AsNoTracking()
                .Include(c => c.Stores)
                .FirstOrDefaultAsync(c => c.Id == id);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company with ID {Id}", id);
            throw;
        }
    }
    public async Task<List<Guid>> GetSellersForCompanyAsync(Guid companyId)
    {
        // Sellers who belong to a store of this company
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Seller
                        && u.StoreId != null
                        && u.Store!.CompanyId == companyId)
            .Select(u => u.Id)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(CompanyDto dto)
    {
        try
        {
            var entity = MapToEntity(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _dbContext.Companies.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company {Name}", dto.Name);
            throw;
        }
    }
    public async Task<CompanyDto?> GetByNumber_of_contractAsync(string Number_of_contract)
    {
        try
        {
            var entity = await _dbContext.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Number_of_contract == Number_of_contract);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company with Name of contract {Number_of_contract}", Number_of_contract);
            throw;
        }
    }
    public async Task<CompanyDto?> GetByINNAsync(string inn)
    {
        try
        {
            var entity = await _dbContext.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.INN == inn);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company with INN {INN}", inn);
            throw;
        }
    }
    public async Task<CompanyDto?> GetByUserNameAsync(string companyName)
    {
        try
        {
            var entity = await _dbContext.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserName == companyName);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company with UserName {UserName}", companyName);
            throw;
        }
    }
    public async Task<CompanyDto?> GetByBinAsync(string bin)
    {
        try
        {
            var entity = await _dbContext.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.BIN == bin);
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company with BIN {BIN}", bin);
            throw;
        }
    }
    public async Task<bool> UpdateAsync(CompanyDto dto)
    {
        try
        {
            var entity = await _dbContext.Companies.FindAsync(dto.Id);
            if (entity == null)
            {
                return false;
            }

            entity.Name = dto.Name;
            entity.ContactEmail = dto.ContactEmail;
            entity.UserName = dto.UserName;
            entity.ContactPhone = dto.ContactPhone;
            entity.BonusBalance = dto.BonusBalance;
            entity.Status = dto.Status;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Number_of_contract = dto.Number_of_contract;
            entity.Date_of_contract = dto.Date_of_contract;
            entity.BonusBalance = dto.BonusBalance;
            entity.FiatBalance = dto.FiatBalance;
            entity.OriginalBonusBalance = dto.OriginalBonusBalance;
            entity.Region = dto.Region;
            entity.City = dto.City;
            entity.Status = CompanyStatus.Active;
            entity.CreatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company with ID {Id}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.Companies.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _dbContext.Companies.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateBalanceAsync(Guid companyId, decimal newBalance, decimal expectedCurrentBalance)
    {
        try
        {
            var result = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE bonus.companies
                SET ""BonusBalance"" = {newBalance}, ""UpdatedAt"" = {DateTime.UtcNow}
                WHERE ""Id"" = {companyId} AND ""BonusBalance"" = {expectedCurrentBalance}");
            
            if (result == 0)
                throw new ConcurrencyException($"Company {companyId} balance update conflict — expected value was {expectedCurrentBalance}");

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for company with ID {Id}", companyId);
            throw;
        }
    }
    public async Task<bool> UpdateFiatBalanceAsync(Guid companyId, decimal newFiatBalance, decimal expectedCurrentFiatBalance)
    {
        try
        {
            var result = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE bonus.companies
                SET ""FiatBalance"" = {newFiatBalance}, ""UpdatedAt"" = {DateTime.UtcNow}
                WHERE ""Id"" = {companyId} AND ""FiatBalance"" = {expectedCurrentFiatBalance}");
            
            if (result == 0)
                throw new ConcurrencyException($"Company {companyId} fiat balance update conflict — expected value was {expectedCurrentFiatBalance}");

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fiat balance for company with ID {Id}", companyId);
            throw;
        }
    }
    public async Task<bool> CreditBalanceAsync(Guid companyId, decimal amount)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
        if (company == null)
            return false;

        company.BonusBalance += amount;
        company.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<bool> CreditFiatBalanceAsync(Guid companyId, decimal amount)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
        if (company == null)
            return false;

        company.FiatBalance += amount;
        company.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    } 
    public async Task<bool> UpdateStatusAsync(Guid companyId, CompanyStatus status)
    {
        try
        {
            var entity = await _dbContext.Companies.FindAsync(companyId);
            if (entity == null)
            {
                return false;
            }

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for company with ID {Id}", companyId);
            throw;
        }
    }

    public async Task<IEnumerable<CompanyDto>> GetCompaniesByStatusAsync(CompanyStatus status)
    {
        try
        {
            var entities = await _dbContext.Companies.AsNoTracking()
                .Where(c => c.Status == status)
                .ToListAsync();

            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies with status {Status}", status);
            throw;
        }
    }

    public async Task<decimal> GetOriginalBalanceAsync(Guid companyId)
    {
        try
        {
            var originalBalance = await _dbContext.Companies.AsNoTracking()
                .Where(c => c.Id == companyId)
                .Select(c => c.OriginalBonusBalance)
                .FirstOrDefaultAsync();

            return originalBalance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving original balance for company with ID {Id}", companyId);
            throw;
        }
    }
    public async Task<CompanyDto?> FindContractAsync(string? bin, string? Number_of_contract, string? inn, string? name)
    {
        try
        {
            var query = _dbContext.Companies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(bin))
                query = query.Where(c => c.BIN == bin);
            
            if (!string.IsNullOrWhiteSpace(Number_of_contract))
                query = query.Where(c => c.Number_of_contract == Number_of_contract);
            
            if (!string.IsNullOrWhiteSpace(inn))
                query = query.Where(c => c.INN == inn);
            
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name == name);

            var entity = await query.FirstOrDefaultAsync();
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding contract with BIN {Bin}, Number_of_contract {Number_of_contract}, INN {Inn}, Name {Name}", bin, Number_of_contract, inn, name);
            throw;
        }
    }

    public async Task<bool> ResetToOriginalBalanceAsync(Guid companyId)
    {
        try
        {
            var entity = await _dbContext.Companies.FindAsync(companyId);
            if (entity == null)
            {
                return false;
            }

            entity.BonusBalance = entity.OriginalBonusBalance;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting balance for company with ID {Id}", companyId);
            throw;
        }
    }

    public async Task<IEnumerable<CompanySummaryDto>> GetCompanySummariesAsync()
    {
        try
        {
            var companies = await _dbContext.Companies.AsNoTracking()
                .Include(c => c.Stores)
                .ToListAsync();

            var summaries = new List<CompanySummaryDto>();

            foreach (var company in companies)
            {
                // Get transaction volume - this is a more complex calculation
                var transactionVolume = await _dbContext.BonusTransactions
                    .Where(t => t.CompanyId == company.Id && t.Status == TransactionStatus.Completed)
                    .SumAsync(t => t.BonusAmount);

                summaries.Add(new CompanySummaryDto
                {
                    Id = company.Id,
                    FrontendId = company.FrontendId,
                    Name = company.Name,
                    BonusBalance = company.BonusBalance,
                    TransactionVolume = transactionVolume,
                    StoreCount = company.Stores.Count,
                    Status = company.Status,
                  
                });
            }

            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company summaries");
            throw;
        }
    }

    private CompanyDto MapToDto(CompanyEntity entity)
    {
        return new CompanyDto
        {
            Id = entity.Id,
            FrontendId = entity.FrontendId,
            Name = entity.Name,
            UserName = entity.UserName,
            ContactEmail = entity.ContactEmail,
            ContactPhone = entity.ContactPhone,
            InitialBonusBalance = entity.InitialBonusBalance,
            BIN = entity.BIN,
            INN = entity.INN,
            Date_of_contract = entity.Date_of_contract,
            Number_of_contract = entity.Number_of_contract,
            Region = entity.Region,
            City = entity.City,
            PasswordHash = entity.PasswordHash,
            BonusBalance = entity.BonusBalance,
            FiatBalance = entity.FiatBalance,
            OriginalBonusBalance = entity.OriginalBonusBalance,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            Stores = entity.Stores?.Select(s => new StoreDto
            {
                Id = s.Id,
                FrontendId = s.FrontendId,
                CompanyId = s.CompanyId,
                Name = s.Name,
                Address = s.Address,
                Email = s.Email,
                MallId = s.MallId,
                Floor = s.Floor,
                Row = s.Row,
                Number = s.Number,
                City = s.City,
                Region = s.Region,
                CategoryId = s.CategoryId,
                WorkingHours = s.WorkingHours,
                ContactPhone = s.ContactPhone,
                Status = s.Status,
            }).ToList() ?? new List<StoreDto>()
        };
    }

    private CompanyEntity MapToEntity(CompanyDto dto)
    {
        return new CompanyEntity
        {
            Id = dto.Id,
            FrontendId = dto.FrontendId,
            Name = dto.Name,
            UserName = dto.UserName,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            PasswordHash = dto.PasswordHash,
            BonusBalance = dto.BonusBalance,
            FiatBalance = dto.FiatBalance,
            BIN = dto.BIN,
            INN = dto.INN,
            Date_of_contract = dto.Date_of_contract,
            Number_of_contract = dto.Number_of_contract,
            Region = dto.Region,
            City = dto.City,
            OriginalBonusBalance = dto.OriginalBonusBalance,
            InitialBonusBalance = dto.InitialBonusBalance,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
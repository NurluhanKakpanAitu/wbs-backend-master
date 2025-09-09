using BonusSystem.Core.Exceptions;
using BonusSystem.Core.Repositories;
using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;

public class EntityFrameworkUserRepository : IUserRepository
{
    private readonly BonusSystemContext _dbContext;
    private readonly ILogger<EntityFrameworkUserRepository> _logger;

    public EntityFrameworkUserRepository(BonusSystemContext dbContext, ILogger<EntityFrameworkUserRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    } 

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        try
        {
            var entities = await _dbContext.Users.AsNoTracking().ToListAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id & u.IsDeleted == false);
            
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {Id}", id);
            throw;
        }
    }
    public async Task<UserDto?> GetByFrontendIdAsync(string frontendId)
    { 
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.FrontendId == frontendId & u.IsDeleted == false);
        if (user == null)
        {
            throw new ArgumentException($"User with frontendId {frontendId} not found");
        }
        return user != null ? MapToDto(user) : null;
    }
    public async Task<int> RemoveAllBonusesFromAllUsersAsync()
    {
        try
        {
            var users = await _dbContext.Users.ToListAsync();

            if (!users.Any())
            {
                _logger.LogInformation("No users found to remove bonuses from.");
                return 0;
            }

            foreach (var user in users)
            {
                if (user.BonusBalance > 0)
                {
                    user.BonusBalance = 0;

                    _dbContext.BonusTransactions.Add(new BonusTransactionEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        BonusAmount = -user.BonusBalance,
                        Timestamp = DateTime.UtcNow,
                        Description = "Quarterly bonus reset"
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Bonuses set to 0 for {Count} users.", users.Count);
            return users.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting bonuses for all users.");
            throw;
        }
    }
    
    public async Task<Guid> CreateAsync(UserDto dto)
    {
        try
        {
            var entity = MapToEntity(dto);
            entity.CreatedAt = DateTime.UtcNow;
            
            await _dbContext.Users.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", dto.Username);
            throw;
        }
    }

    public async Task<bool> UpdatePasswordAsync(Guid userid, string new_password)
    {
        var user = await _dbContext.Users.FindAsync(userid);
        if (user == null)
            return false;
        user.PasswordHash = new_password;
        _dbContext.Users.Update(user); 
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePincodeStatus(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            return false;
        user.PincodeSet = true;
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return true; 
    }

    public async Task<bool> UpdateAsync(UserDto dto)
    {
        _logger.LogInformation("Starting update for user {Id}", dto.Id);

        var user = await _dbContext.Users.FindAsync(dto.Id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {Id} not found", dto.Id);
            return false;
        }

        if (user.FirstName != dto.FirstName) user.FirstName = dto.FirstName;
        if (user.LastName != dto.LastName) user.LastName = dto.LastName;
        if (user.City != dto.City) user.City = dto.City;
        if (user.Region != dto.Region) user.Region = dto.Region;
        if (user.INN != dto.INN) user.INN = dto.INN;
        if (user.Phone != dto.Phone) user.Phone = dto.Phone;
        if (user.DeviceToken != dto.DeviceToken) user.DeviceToken = dto.DeviceToken;

        try
        {
            _dbContext.Users.Update(user); 
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User {Id} updated successfully", dto.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating user {Id}", dto.Id);
            return false;
        }
    }


    public async Task<List<UserDto>> GetUnverifiedOlderThanAsync(DateTime cutoff)
    {
        return await _dbContext.Users
            .Where(u => !u.IsEmailVerified && u.CreatedAt < cutoff)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FrontendId = u.FrontendId,
                Username = u.Username,
                Email = u.Email,
                PasswordHash = u.PasswordHash,
                Phone = u.Phone,
                City = u.City,
                Region = u.Region,
                FirstName = u.FirstName,
                LastName = u.LastName,
                INN = u.INN,
                Role = u.Role,
                BonusBalance = u.BonusBalance,
                CompanyId = u.CompanyId
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Users
            .Where(u => u.Username == username)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FrontendId = u.FrontendId,
                Username = u.Username,
                Email = u.Email,
                PasswordHash = u.PasswordHash,
                Phone = u.Phone,
                City = u.City,
                Region = u.Region,
                FirstName = u.FirstName,
                LastName = u.LastName,
                INN = u.INN,
                Role = u.Role,
                BonusBalance = u.BonusBalance,
                CompanyId = u.CompanyId
            })
            .FirstOrDefaultAsync();
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await _dbContext.Users.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            entity.IsDeleted = true;
            _dbContext.Users.Update(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {Id}", id);
            throw;
        }
    }


    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        try
        {
            var entity = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email {Email}", email);
            throw;
        }
    }

    public async Task<bool> UpdateBalanceAsync(Guid userId, decimal newBalance, decimal expectedCurrentBalance)
    {
        try
        {
            var result = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE bonus.users
                SET ""BonusBalance"" = {newBalance}
                WHERE ""Id"" = {userId} AND ""BonusBalance"" = {expectedCurrentBalance}");

            if (result == 0)
                throw new InvalidOperationException($"User {userId} balance update conflict — expected value was {expectedCurrentBalance}");
                
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for user with ID {Id}", userId);
            throw;
        }
    }
    public async Task<PagedResult<CombinedTransactionDto>> AllTransactionsForUserAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for transaction lookup.", userId);
                return new PagedResult<CombinedTransactionDto>
                {
                    Items = new List<CombinedTransactionDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
            var bonusTransactions = user.Role switch
            {
                UserRole.Buyer => await (from t in _dbContext.BonusTransactions.Where(t => t.UserId == userId)
                                       select new CombinedTransactionDto
                                       {
                                           Id = t.Id,
                                           Timestamp = t.Timestamp,
                                           Amount = t.TotalCost,
                                           BonusAmount = t.BonusAmount,
                                           CashbackAmount = 0,
                                           Type = "Bonus",
                                           Description = t.Description,
                                           Status = t.Status.ToString(),
                                           ClientFrontendId = user.FrontendId,
                                           ClientName = $"{user.FirstName} {user.LastName}".Trim(),
                                           RelatedBonusTransactionId = null
                                       }).ToListAsync(),
                UserRole.Seller => await (from t in _dbContext.BonusTransactions.Where(t => t.SellerId == userId)
                                        select new CombinedTransactionDto
                                        {
                                            Id = t.Id,
                                            Timestamp = t.Timestamp,
                                            Amount = t.TotalCost,
                                            BonusAmount = t.BonusAmount,
                                            CashbackAmount = 0,
                                            Type = "Bonus",
                                            Description = t.Description,
                                            Status = t.Status.ToString(),
                                            ClientFrontendId = user.FrontendId,
                                            ClientName = $"{user.FirstName} {user.LastName}".Trim(),
                                            RelatedBonusTransactionId = null
                                        }).ToListAsync(),
                _ => await (from t in _dbContext.BonusTransactions.Where(t => t.StoreId == user.StoreId)
                           join u in _dbContext.Users on t.UserId equals u.Id
                           select new CombinedTransactionDto
                           {
                               Id = t.Id,
                               Timestamp = t.Timestamp,
                               Amount = t.TotalCost,
                               BonusAmount = t.BonusAmount,
                               CashbackAmount = 0,
                               Type = "Bonus",
                               Description = t.Description,
                               Status = t.Status.ToString(),
                               ClientFrontendId = u.FrontendId,
                               ClientName = $"{u.FirstName} {u.LastName}".Trim(),
                               RelatedBonusTransactionId = null
                           }).ToListAsync()
            };

            var fiatTransactions = user.Role switch
            {
                UserRole.Buyer => await (from t in _dbContext.FiatTransactions.Where(t => t.UserId == userId)
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
                                           ClientFrontendId = user.FrontendId,
                                           ClientName = $"{user.FirstName} {user.LastName}".Trim(),
                                           RelatedBonusTransactionId = t.RelatedBonusTransactionId
                                       }).ToListAsync(),
                UserRole.Seller => await (from t in _dbContext.FiatTransactions.Where(t => t.SellerId == userId)
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
                                            ClientFrontendId = user.FrontendId,
                                            ClientName = $"{user.FirstName} {user.LastName}".Trim(),
                                            RelatedBonusTransactionId = t.RelatedBonusTransactionId
                                        }).ToListAsync(),
                _ => await (from t in _dbContext.FiatTransactions.Where(t => t.StoreId == user.StoreId)
                           join u in _dbContext.Users on t.UserId equals u.Id
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
                               ClientName = $"{u.FirstName} {u.LastName}".Trim(),
                               RelatedBonusTransactionId = t.RelatedBonusTransactionId
                           }).ToListAsync()
            };

            // Combine and sort in memory
            var combinedTransactions = bonusTransactions
                .Union(fiatTransactions)
                .OrderByDescending(t => t.Timestamp)
                .ToList();

            var totalCount = combinedTransactions.Count;
            var pagedTransactions = combinedTransactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<CombinedTransactionDto>
            {
                Items = pagedTransactions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all transactions for user with ID {UserId}", userId);
            throw;
        }
    }
    public async Task<PagedResult<TransferHistoryDto>> GetReplenishmentsAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var query = _dbContext.Transfers
                .Where(t => t.RecipientId == userId && t.Type == TransactionType.Replenishment)
                .Include(t => t.Recipient);

            var totalCount = await query.CountAsync();

            var transfers = await query
               .OrderByDescending(t => t.Timestamp)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .Select(t => new TransferHistoryDto
               {
                   Id = t.Id,
                   SenderId = t.SenderId,
                   RecipientId = t.RecipientId,
                   TransactionId = t.Id,
                   Amount = t.Amount,
                   Type = t.Type,
                   Status = t.Status,
                   CommissionPercent = t.CommissionPercent,
                   Timestamp = t.Timestamp,
                   IsIncoming = t.RecipientId == userId,
                   RecipientUserName = t.RecipientId == userId ? t.Sender.FirstName + " " + t.Sender.LastName : t.Recipient.FirstName + " " + t.Recipient.LastName,
                   RecipientUserFrontendId = t.RecipientId == userId ? t.Sender.FrontendId : t.Recipient.FrontendId
               })
               .ToListAsync();
            return new PagedResult<TransferHistoryDto>
            {
                Items = transfers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {  
            _logger.LogError(ex, "Error retrieving replenishments for user with ID {UserId}", userId);
            throw;
        }
    }
    public async Task<PagedResult<TransferHistoryDto>> GetTransfersForUserAsync(Guid userId, int page, int pageSize)
    {
        try
        {
            var query = _dbContext.Transfers
                .Where(t => t.SenderId == userId || t.RecipientId == userId && t.Type != TransactionType.Replenishment)
                .Include(t => t.Sender)
                .Include(t => t.Recipient);

            var totalCount = await query.CountAsync();

            var transfers = await query
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransferHistoryDto
                {
                    Id = t.Id,
                    SenderId = t.SenderId,
                    RecipientId = t.RecipientId,
                    TransactionId = t.Id,
                    Amount = t.Amount,
                    Type = t.Type,
                    Status = t.Status,
                    CommissionPercent = t.CommissionPercent,
                    Timestamp = t.Timestamp,
                    IsIncoming = t.RecipientId == userId,
                    RecipientUserName = t.RecipientId == userId ? t.Sender.FirstName + " " + t.Sender.LastName : t.Recipient.FirstName + " " + t.Recipient.LastName,
                    RecipientUserFrontendId = t.RecipientId == userId ? t.Sender.FrontendId : t.Recipient.FrontendId
                })
                .ToListAsync();
            return new PagedResult<TransferHistoryDto>
            {
                Items = transfers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transfers for user with ID {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateFiatBalanceAsync(Guid userId, decimal newFiatBalance, decimal expectedCurrentFiatBalance)
    {
        try
        {
            var result = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE bonus.users
                SET ""FiatBalance"" = {newFiatBalance}
                WHERE ""Id"" = {userId} AND ""FiatBalance"" = {expectedCurrentFiatBalance}");

            if (result == 0)
                throw new ConcurrencyException($"User {userId} fiat balance update conflict — expected value was {expectedCurrentFiatBalance}");

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for user with ID {Id}", userId);
            throw;
        }
    }

    public async Task<UserRole> GetUserRoleAsync(Guid userId)
    {
        try
        {
            var role = await _dbContext.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role for user with ID {Id}", userId);
            throw;
        }
    }

    public async Task<bool> IsUserExistsByEmailAsync(string email)
    {
        try
        {
            return await _dbContext.Users.AsNoTracking()
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists with email {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role)
    {
        try
        {
            var entities = await _dbContext.Users.AsNoTracking()
                .Where(u => u.Role == role)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with role {Role}", role);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetUsersByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var entities = await _dbContext.Users.AsNoTracking()
                .Where(u => u.CompanyId == companyId)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for company with ID {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<bool> AssignUserToCompanyAsync(Guid userId, Guid companyId)
    {
        try
        {
            // Check if the company exists
            var companyExists = await _dbContext.Companies.AnyAsync(c => c.Id == companyId);
            if (!companyExists)
            {
                _logger.LogWarning("Cannot assign user to non-existent company. Company ID: {CompanyId}", companyId);
                return false;
            }
            
            // Find the user
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Cannot assign non-existent user to company. User ID: {UserId}", userId);
                return false;
            }
            
            // Only allow assigning Company or Seller roles to a company
            if (user.Role != UserRole.Company && user.Role != UserRole.Seller)
            {
                _logger.LogWarning("Cannot assign user with role {Role} to company. Only Company and Seller roles can be assigned.", user.Role);
                return false;
            }
            
            // Assign company ID
            user.CompanyId = companyId;
            await _dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserId} to company {CompanyId}", userId, companyId);
            throw;
        }
    }

    public async Task<UserDto?> GetCompanyAdminUserByCompanyIdAsync(Guid companyId)
    {
        try
        {
            // Find the company admin (user with Company role and the specified company ID)
            var entity = await _dbContext.Users.AsNoTracking()
                .Where(u => u.CompanyId == companyId && u.Role == UserRole.Company)
                .FirstOrDefaultAsync();
            
            return entity != null ? MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin user for company with ID {CompanyId}", companyId);
            throw;
        }
    }
    public async Task<bool> SetEmailVerifiedAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            return false;
        user.IsEmailVerified = true;
        user.VerificationCode = null;
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetVerificationCodeAsync(string email, string code)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            return false;
        user.VerificationCode = code;
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    private UserDto MapToDto(UserEntity entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            FrontendId = entity.FrontendId,
            PasswordHash = entity.PasswordHash,
            Phone = entity.Phone,
            City = entity.City,
            Region = entity.Region,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            INN = entity.INN,
            Username = entity.Username,
            Email = entity.Email,
            Role = entity.Role,
            BonusBalance = entity.BonusBalance,
            CompanyId = entity.CompanyId,
            FiatBalance = entity.FiatBalance,
            VerificationCode = entity.VerificationCode,
            IsEmailVerified = entity.IsEmailVerified,
            PincodeSet = entity.PincodeSet,
            StoreId = entity.StoreId,
            DeviceToken = entity.DeviceToken,
            IsDeleted = entity.IsDeleted
        };
    }

    private UserEntity MapToEntity(UserDto dto)
    {
        return new UserEntity
        {
            Id = dto.Id,
            FrontendId = dto.FrontendId,
            PasswordHash = dto.PasswordHash,
            Phone = dto.Phone,
            City = dto.City,
            Region = dto.Region,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            INN = dto.INN,
            Username = dto.Username,
            Email = dto.Email,
            Role = dto.Role,
            BonusBalance = dto.BonusBalance,
            CompanyId = dto.CompanyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            FiatBalance = dto.FiatBalance,
            VerificationCode = dto.VerificationCode,
            IsEmailVerified = dto.IsEmailVerified,
            PincodeSet = dto.PincodeSet,
            StoreId = dto.StoreId,
            IsDeleted = dto.IsDeleted
        };
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _dbContext.Users.CountAsync();
    }

    public async Task<List<UserDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        try
        {
            var idList = ids?.Where(x => x != Guid.Empty).Distinct().ToList() ?? new List<Guid>();
            if (idList.Count == 0)
                return new List<UserDto>();

            var entities = await _dbContext.Users
                .AsNoTracking()
                .Where(u => idList.Contains(u.Id))
                .ToListAsync();

            return entities.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by IDs {Ids}", ids);
            throw;
        }
    }
    public async Task<bool> UpdateStoreAssignmentAsync(Guid userId, Guid? storeId)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found when assigning to store {StoreId}", userId, storeId);
                return false;
            }

            user.StoreId = storeId;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Assigned user {UserId} to store {StoreId}", userId, storeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserId} to store {StoreId}", userId, storeId);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetUsersByStoreIdAsync(Guid storeId)
    {
        try
        {
            var entities = await _dbContext.Users.AsNoTracking()
                .Where(u => u.StoreId == storeId)
                .ToListAsync();
            
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for store with ID {StoreId}", storeId);
            throw;
        }
    }
}

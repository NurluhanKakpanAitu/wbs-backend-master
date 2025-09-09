using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Core.Email; 
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using BonusSystem.Core.Common.IDGenerator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;

namespace BonusSystem.Core.Services.Implementations.BFF;

/// <summary>
/// BFF service for Admin role
/// </summary>
public class AdminBffService : BaseBffService, IAdminBffService
{
    private readonly EmailSenderService _emailSender;
    private readonly IIDGenerator _idGen;
    public AdminBffService(
        IDataService dataService,
        EmailSenderService emailSender,
        IAuthenticationService authService,
        IIDGenerator idGen)
        : base(dataService, authService)
    {
        _idGen = idGen ?? throw new ArgumentNullException(nameof(idGen));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    /// <summary>
    /// Retrieves the user context for a specified user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UserDto"/> with the user's details.</returns>
    /// <exception cref="ArgumentException">Thrown if a user with the specified ID is not found.</exception>

    public async Task<UserDto> GetUserContextAsync(Guid userId)
    {
        var user = await _dataService.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Phone = user.Phone,
            FirstName = user.FirstName,
            LastName = user.LastName,
            City = user.City,
            Region = user.Region,
            INN = user.INN,
            Role = user.Role,
            BonusBalance = user.BonusBalance,
            CompanyId = user.CompanyId,
            FiatBalance = user.FiatBalance,
            VerificationCode = user.VerificationCode,
            CreatedAt = user.CreatedAt,
            IsEmailVerified = user.IsEmailVerified,
            FrontendId = user.FrontendId,
            PincodeSet = user.PincodeSet
        };
    }
    /// <summary>
    /// Registers a new company
    /// </summary>
    public async Task<CompanyRegistrationResultDto> RegisterCompanyAsync(CompanyRegistrationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.PasswordHash))
        {
            return new CompanyRegistrationResultDto
            {
                Success = false,
                ErrorMessage = "UserName and PasswordHash are required"
            };
        }
        var frontendId = _idGen.NewId();

        if (string.IsNullOrWhiteSpace(frontendId))
            throw new Exception("Generated FrontendId is null or empty");

        var company = new CompanyDto
        {
            Id = Guid.NewGuid(),
            FrontendId = frontendId,
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            BonusBalance = 0.00m,
            OriginalBonusBalance = 0.00m,
            FiatBalance = 0.00m,
            InitialBonusBalance = 0.00m,
            Status = CompanyStatus.Active,
            CreatedAt = DateTime.UtcNow,
            BIN = request.BIN,
            INN = request.INN,
            Date_of_contract = request.Date_of_contract,
            Number_of_contract = request.Number_of_contract,
            City = request.City,
            Region = request.Region,
            UserName = request.UserName,
            PasswordHash = request.PasswordHash,
            Stores = new List<StoreDto>()
        };

        await _dataService.Companies.CreateAsync(company);
        return new CompanyRegistrationResultDto
        {
            Company = company,
            Success = true
        };
    }
    /// <summary>
    /// Finds user by frontendId
    /// </summary>
    /// <param name="frontendId">FrontendId of the user</param>
    /// <returns>UserDto or throws ArgumentException if not found</returns>
    public async Task<UserDto> FindUserByFrontendIdAsync(string frontendId)
    {
        var user = await _dataService.Users.GetByFrontendIdAsync(frontendId);
        if (user == null)
        {
            throw new ArgumentException($"User with frontendId {frontendId} not found");
        }
        return user;
    }
    /// <summary>
    /// Finds user by email
    /// </summary>
    /// <param name="email">Email of the user</param>
    /// <returns>UserDto or throws ArgumentException if not found</returns>
    public async Task<UserDto> FindUserAsync(string email)
    {
        var user = await _dataService.Users.GetByEmailAsync(email);
        if (user == null)
        {
            throw new ArgumentException($"User with email {email} not found");
        }
        return user;
    }
    
    /// <summary>
    /// Appoints a user as admin of a company.
    /// </summary>
    /// <param name="request">AppointAdminDto with the company ID and user ID</param>
    /// <returns>A tuple with a boolean indicating whether the operation was successful and a string with a message</returns>
    public async Task<(bool, string)> AppointAdminAsync(AppointAdminDto request)
    {
        var company = await _dataService.Companies.GetByIdAsync(request.CompanyId);
        if (company == null)
        {
            return (false, "Company not found");
        }

        var user = await _dataService.Users.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return (false, "User not found");
        }

        user.CompanyId = request.CompanyId;
        await _dataService.Users.UpdateAsync(user);
        return (true, "Ok");
    }

    private async Task<string> GenerateTokenAsync(Guid userId, UserRole role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("SuperSecretKeyForDevPurposesOnly1234567890!");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role.ToString())
        };

        if (role == UserRole.Company)
        {
            var user = await _dataService.Users.GetByIdAsync(userId);
            if (user != null && user.CompanyId.HasValue)
            {
                claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    } 
    
    /// <summary>
    /// Gets the permitted actions for an admin
    /// </summary>
    public override async Task<IEnumerable<PermittedActionDto>> GetPermittedActionsAsync(Guid userId)
    {
        var role = await _dataService.Users.GetUserRoleAsync(userId);
        if (role != UserRole.SystemAdmin)
        {
            return Enumerable.Empty<PermittedActionDto>();
        }

        return new List<PermittedActionDto>
        {
            new()
            {
                ActionName = "RegisterCompany", Description = "Register a new company",
                Endpoint = "/api/admin/companies"
            },
            new()
            {
                ActionName = "UpdateCompanyStatus", Description = "Update company status",
                Endpoint = "/api/admin/companies/{id}/status"
            },
            new()
            {
                ActionName = "ModerateStore", Description = "Moderate a store",
                Endpoint = "/api/admin/stores/{id}/moderate"
            },
            new()
            {
                ActionName = "CreditCompanyBalance", Description = "Credit company balance",
                Endpoint = "/api/admin/companies/{id}/credit"
            },
            new()
            {
                ActionName = "GetSystemTransactions", Description = "Get system transactions",
                Endpoint = "/api/admin/transactions"
            },
            new()
            {
                ActionName = "SendNotification", Description = "Send system notification",
                Endpoint = "/api/admin/notifications"
            },
            new()
            {
                ActionName = "GetSystemFiatTransactions", Description = "Get system fiat transactions",
                Endpoint = "/api/admin/fiatTransactions"
            }
        };
    }
    private Task SendWelcomeEmailAsync(string email)
    {
        return _emailSender.SendEmailAsync(
            email,
            "Welcome to the Company",
            "Your account has been created successfully.\nhttps://world-bonus-system-6d14b.web.app/partners/login"
        );
    }

    /// <summary>
    /// Sends a company URL to the company's contact email address after registration or moderation.
    /// </summary>
    /// <param name="companyId">The ID of the company to send the URL to</param>
    /// <remarks>
    /// If the company's contact email address does not already have an associated user account, a new user account will be created.
    /// </remarks>
    public async Task<AuthResult> SendCompanyURLAsync(Guid companyId)
    {
        var company = await _dataService.Companies.GetByIdAsync(companyId);
        if (company == null)
        {
            throw new Exception($"Company with id {companyId} not found");
        }

        if (string.IsNullOrWhiteSpace(company.ContactEmail))
        {
            throw new Exception($"Company with id {companyId} has no contact email.");
        }

        if (string.IsNullOrWhiteSpace(company.PasswordHash))
        {
            throw new Exception($"Company with id {companyId} has no password hash.");
        }

        var userOfCompany = await _dataService.Users.GetByEmailAsync(company.ContactEmail);

        if (userOfCompany == null)
        {
            var frontendId = _idGen.NewId();
            if (string.IsNullOrWhiteSpace(frontendId))
            {
                throw new Exception("Failed to generate frontend ID");
            }

            var companyAdmin = new UserDto
            {
                Id = Guid.NewGuid(),
                FrontendId = frontendId,
                PasswordHash = company.PasswordHash,
                Username = company.Name,
                Email = company.ContactEmail,
                Role = UserRole.Company,
                BonusBalance = 0.00m,
                CompanyId = company.Id,
                INN = company.INN,
                FiatBalance = 0.00m,
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = true,
                Phone = company.ContactPhone,
                City = company.City,
                FirstName = string.Empty,
                LastName = string.Empty,
                Region = company.Region 
            };

            await _dataService.Users.CreateAsync(companyAdmin);

            await SendWelcomeEmailAsync(company.ContactEmail); 

            return new AuthResult
            {
                Success = true,
                Token = await GenerateTokenAsync(companyAdmin.Id, companyAdmin.Role),
                UserId = companyAdmin.Id,
                Role = companyAdmin.Role
            };
        }

        else
        {
            await SendWelcomeEmailAsync(company.ContactEmail); 

            return new AuthResult
            {
                Success = true,
                Token = await GenerateTokenAsync(userOfCompany.Id, userOfCompany.Role),
                UserId = userOfCompany.Id,
                Role = userOfCompany.Role
            };
        }
    }
    
    /// <summary>
    /// Updates the status of a company
    /// </summary>
    public async Task<bool> UpdateCompanyStatusAsync(Guid companyId, CompanyStatus status)
    {
        return await _dataService.Companies.UpdateStatusAsync(companyId, status);
    }

    /// <summary>
    /// Moderates a store (approves or rejects)
    /// </summary>
    public async Task<bool> ModerateStoreAsync(Guid storeId, bool isApproved)
    {
        var store = await _dataService.Stores.GetByIdAsync(storeId);
        if (store == null || store.Status != StoreStatus.PendingApproval)
        {
            return false;
        }

        var newStatus = isApproved ? StoreStatus.Active : StoreStatus.Inactive;
        return await _dataService.Stores.UpdateStatusAsync(storeId, newStatus);
    }


    /// <summary>
    /// Credits a company's bonus balance
    /// </summary>
    public async Task<bool> CreditCompanyBalanceAsync(Guid companyId, decimal amount)
    {
        var success = await _dataService.Companies.CreditBalanceAsync(companyId, amount);
        if (!success)
            return false;

        var transaction = new TransactionDto
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            BonusAmount = amount,
            TotalCost = 0.00m,
            Type = TransactionType.AdminAdjustment,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed,
            Description = "Admin credit adjustment"
        };

        await _dataService.Transactions.CreateAsync(transaction);
        return true;
    }
    public async Task<bool> ReplenishFiatCompanyAsync(Guid companyId, decimal amount)
    { 
        var success = await _dataService.Companies.CreditFiatBalanceAsync(companyId, amount);
        if (!success)
            return false;

        var transaction = new FiatReplenishmentCompanyBalanceDto
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Amount = amount,
            Timestamp = DateTime.UtcNow
        };

        await _dataService.FiatReplenishmentCompanyBalances.CreateAsync(transaction);
        return true;
    }
    /// <summary>
    /// Gets system transactions
    /// </summary>
    public async Task<IEnumerable<TransactionDto>> GetSystemTransactionsAsync(Guid? companyId = null,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        IEnumerable<TransactionDto> transactions;

        if (companyId.HasValue)
        {
            transactions = await _dataService.Transactions.GetTransactionsByCompanyIdAsync(companyId.Value);
        }
        else
        {
            transactions = await _dataService.Transactions.GetAllAsync();
        }

        // Apply date filters if provided
        if (startDate.HasValue)
        {
            transactions = transactions.Where(t => t.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            transactions = transactions.Where(t => t.Timestamp <= endDate.Value);
        }

        return transactions.OrderByDescending(t => t.Timestamp);
    }

    /// <summary>
    /// Sends a system notification
    /// </summary>
    public async Task<bool> SendSystemNotificationAsync(Guid? recipientId, string message, NotificationType type)
    {
        if (recipientId.HasValue)
        {
            // Send to specific recipient
            return await _dataService.Notifications.SendNotificationAsync(recipientId.Value, message, type);
        }
        else
        {
            // Send to all users of a specific role based on notification type
            UserRole targetRole = type switch
            {
                NotificationType.Transaction => UserRole.Buyer,
                NotificationType.AdminMessage => UserRole.SystemAdmin,
                _ => UserRole.Buyer // Default to buyers for other types
            };

            return await _dataService.Notifications.SendNotificationToRoleAsync(targetRole, message, type);
        }
    }

    public async Task<List<CompanyFeeResult>> GetTransactionFeesAsync(TransactionFeeRequest request)
    {
        var transactions = await _dataService.Transactions.GetAllAsync();

        if (request.FromDate != null)
            transactions = transactions.Where(t => t.Timestamp >= request.FromDate);

        if (request.EndDate != null)
            transactions = transactions.Where(t => t.Timestamp <= request.EndDate);


        var groupedTransactions = transactions
            .Where(t => t.CompanyId.HasValue)
            .Where(t => t.Status == TransactionStatus.Completed)
            .GroupBy(t => t.CompanyId!.Value)
            .Select(g => new
            {
                CompanyId = g.Key,
                TotalTransactions = g.Count(),
                TotalFee = g.Sum(t => t.TotalCost * request.FeePercent)
            })
            .ToList();

        var companyIds = groupedTransactions.Select(g => g.CompanyId).ToList();
        var companies = (await _dataService.Companies.GetAllAsync())
            .Where(c => companyIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c.Name);

        var result = new List<CompanyFeeResult>();
        foreach (var gr in groupedTransactions)
        {
            companies.TryGetValue(gr.CompanyId, out var companyName);

            result.Add(new CompanyFeeResult
            {
                CompanyId = gr.CompanyId,
                CompanyName = companyName ?? "Unknown",
                TotalTransactions = gr.TotalTransactions,
                TotalFee = gr.TotalFee
            });
        }

        return result;
    }
    /// <summary>
    /// Gets system fiat transactions
    /// </summary>
    public async Task<IEnumerable<FiatTransactionDto>> GetSystemFiatTransactionsAsync(Guid? companyId = null,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        IEnumerable<FiatTransactionDto> transactions;

        if (companyId.HasValue)
        {
            transactions = await _dataService.FiatTransactions.GetFiatTransactionsByCompanyIdAsync(companyId.Value);
        }
        else
        {
            transactions = await _dataService.FiatTransactions.GetAllAsync();
        }

        // Apply date filters if provided
        if (startDate.HasValue)
        {
            transactions = transactions.Where(t => t.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            transactions = transactions.Where(t => t.Timestamp <= endDate.Value);
        }

        return transactions.OrderByDescending(t => t.Timestamp);
    }
}
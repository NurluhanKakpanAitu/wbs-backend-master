using BonusSystem.Core.Common.IDGenerator;
using BonusSystem.Core.Email;
using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using BonusSystem.Localization;

namespace BonusSystem.Core.Services.Implementations.BFF;

public class CompanyBffService : ICompanyBffService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDataService _dataService;
    private readonly IIDGenerator _idGenerator;
    private readonly ILogger<CompanyBffService> _logger;
    private readonly EmailSenderService _emailSender;
    private readonly IRealTimeMonitoringService _realTimeMonitoringService;

    public CompanyBffService(
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IStoreRepository storeRepository,
        ITransactionRepository transactionRepository,
        IDataService dataService,
        ILogger<CompanyBffService> logger,
        IIDGenerator idGenerator,
        EmailSenderService emailSender,
        IRealTimeMonitoringService realTimeMonitoringService)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _storeRepository = storeRepository;
        _transactionRepository = transactionRepository;
        _dataService = dataService;
        _idGenerator = idGenerator;
        _logger = logger;
        _emailSender = emailSender;
        _realTimeMonitoringService = realTimeMonitoringService;
    }

    public async Task<UserDto> GetUserContextAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException(Res.Format("Error.UserNotFoundWithId", userId));
        }

        return new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            Role = user.Role,
            BonusBalance = user.BonusBalance,
            FiatBalance = user.FiatBalance,
            VerificationCode = user.VerificationCode,
            CreatedAt = user.CreatedAt,
            IsEmailVerified = user.IsEmailVerified,
            PincodeSet = user.PincodeSet,
            INN = user.INN,
            City = user.City,
            Region = user.Region,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Email = user.Email,
            Username = user.Username,
            FrontendId = user.FrontendId
        };
    }
    public async Task<PagedResult<StoreDto>> GetStoresForCompanyAsync(Guid companyId, int page, int pageSize)
    { 
        return await _storeRepository.GetStoresByCompanyIdPagedAsync(companyId, page, pageSize);
    }
    public async Task<bool> DeleteStoreByIdAsync(Guid storeId)
    {
        var store = await _storeRepository.GetByIdAsync(storeId);
        if (store == null)
            return false;

        var sellers = await _userRepository.GetUsersByStoreIdAsync(storeId);
        foreach (var seller in sellers)
        {
            await _userRepository.UpdateStoreAssignmentAsync(seller.Id, null);
        }

        return await _storeRepository.DeleteAsync(storeId);
    }

    public async Task<bool> SendCommissionNotificationAsync()
    {
        var activeCompanies = await _companyRepository.GetCompaniesByStatusAsync(CompanyStatus.Active);
        _logger.LogInformation("Found {Count} active companies for commission notification.", activeCompanies.Count());

        var anySent = false;

        foreach (var company in activeCompanies)
        {
            if (string.IsNullOrWhiteSpace(company.ContactEmail))
            {
                _logger.LogWarning("Company '{CompanyName}' (ID: {CompanyId}) has no contact email. Notification skipped.", company.Name, company.Id);
                continue;
            }

            var subject = Res.Get("Email.CommissionNotification.Subject");
            var body = Res.Format("Email.CommissionNotification.Body", company.Name);

            try
            {
                await _emailSender.SendEmailAsync(company.ContactEmail, subject, body);
                _logger.LogInformation("Commission notification successfully sent to {Email} for company '{CompanyName}'.", company.ContactEmail, company.Name);
                anySent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send commission notification to {Email} for company '{CompanyName}'.", company.ContactEmail, company.Name);
            }
        }

        return anySent;
    }

    public async Task<bool> UpdateStoreAddressAsync(Guid storeId, string newAddress, string floor, string number, string row)
    {
        var store = await _storeRepository.GetByIdAsync(storeId);
        if (store == null)
            return false;
        var updatedStore = store with { Address = newAddress, Floor = floor, Number = number, Row = row };
        return await _storeRepository.UpdateAsync(updatedStore);
    }
    public async Task<TransactionDto> GetLastTransactionAsync(Guid? companyId)
    {
        if (!companyId.HasValue)
        {
            throw new ArgumentNullException(nameof(companyId), Res.Get("Error.CompanyIdMustBeProvided"));
        }

        // Validate company exists
        var company = await _companyRepository.GetByIdAsync(companyId.Value);
        if (company == null)
        {
            throw new ArgumentException(Res.Format("Error.CompanyNotFoundWithId", companyId), nameof(companyId));
        }

        // Get most recent transaction for this company
        var transactions = await _transactionRepository.GetTransactionsByCompanyIdAsync(companyId.Value);
        var latestTransaction = transactions.OrderByDescending(t => t.Timestamp).FirstOrDefault();

        if (latestTransaction == null)
        {
            // If no transactions exist, return a placeholder
            return new TransactionDto
            {
                Id = Guid.Empty,
                CompanyId = companyId,
                BonusAmount = 0,
                TotalCost = 0,
                Type = TransactionType.AdminAdjustment,
                Timestamp = DateTime.UtcNow,
                Status = TransactionStatus.Completed,
                Description = Res.Get("Info.NoTransactionsFound")
            };
        }   

        return latestTransaction;
    }

    public async Task<MonitoringDto> GetMonitoringAsync(Guid companyId, string companyINN, string companyName, DateTime date)
    {
        var company = companyId != Guid.Empty
            ? await _companyRepository.GetByIdAsync(companyId)
            : !string.IsNullOrWhiteSpace(companyINN)
                ? await _companyRepository.GetByINNAsync(companyINN)
                : !string.IsNullOrWhiteSpace(companyName)
                    ? await _companyRepository.GetByUserNameAsync(companyName)
                    : null;

        if (company == null)
            throw new ArgumentException(Res.Get("Error.CompanyNotFoundByParameters"));

        var companyIdResolved = company.Id;

        var transactions = (await _transactionRepository.GetTransactionsByCompanyIdAsync(companyIdResolved))
            .Where(t => t.Timestamp.Date == date.Date)
            .ToList();

        var fiatTransactions = (await _dataService.FiatTransactions.GetFiatTransactionsByCompanyIdAsync(companyIdResolved))
            .Where(t => t.Timestamp.Date == date.Date)
            .ToList();

        decimal givenBonuses = transactions.Sum(t => t.BonusAmount);
        decimal totalCost = transactions.Sum(t => t.TotalCost);
        decimal commission = transactions.Sum(t => t.CommissionPercent / 100m * t.TotalCost);

        return new MonitoringDto
        {
            CompanyId = companyIdResolved,
            Date = date,
            BonusBalance = Math.Round(company.BonusBalance, 2),
            GivenBonuses = Math.Round(givenBonuses, 2),
            ReceivedBonuses = Math.Round(totalCost, 2),
            Sales = Math.Round(totalCost, 2),
            Commission = Math.Round(commission, 2)
        };
    }

    public async Task<Result<SellerOutput>> GetCompanySellers(Guid companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
            return new Result<SellerOutput>
            {
                Success = false,
                Message = $"Компания с Id {companyId} не найдена"
            };

        var sellerIds = await _companyRepository.GetSellersForCompanyAsync(companyId);

        if (sellerIds.Count == 0)
            return new Result<SellerOutput>
            {
                Success = false,
                Message = "У компании нет продавцов"
            };

        var sellers = new List<SellerOutput>();
        foreach (var sellerId in sellerIds)
        {
            var seller = await _dataService.Users.GetByIdAsync(sellerId);
            if (seller == null)
                return new Result<SellerOutput>
                {
                    Success = false,
                    Message = $"Продавец {sellerId} не найден"
                };

            var store = await _dataService.Stores.GetByIdAsync(seller.StoreId ?? Guid.Empty);
            if (store == null)
                return new Result<SellerOutput>
                {
                    Success = false,
                    Message = $"Магазин {seller.StoreId} не найден"
                };

            sellers.Add(new SellerOutput
            {
                Id = seller.Id,
                Name = seller.Username,
                StoreName = store.Name,
                IsAppoint = seller.StoreId == store.Id
            });
        }

        return new Result<SellerOutput>
        {
            Success = true,
            Data = sellers
        };
    }

    public async Task<bool> RemoveSellerForStore(Guid storeId, Guid sellerId)
    {
        var store = await _dataService.Stores.GetByIdAsync(storeId);
        if (store == null)
            return false;

        var seller = await _dataService.Users.GetByIdAsync(sellerId);
        if (seller == null)
            return false;

        seller.StoreId = Guid.Empty;
        await _dataService.Users.UpdateAsync(seller);
        return true;
    }

    public async Task<IEnumerable<PermittedActionDto>> GetPermittedActionsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException(Res.Format("Error.UserNotFoundWithId", userId), nameof(userId));
        }

        // Define permitted actions based on role
        var permittedActions = new List<PermittedActionDto>();

        if (user.Role == UserRole.Company || user.Role == UserRole.StoreAdmin)
        {
            permittedActions.Add(new PermittedActionDto
            {
                ActionName = "RegisterStore",
                Description = "Register a new store",
                Endpoint = "/api/company/stores"
            });

            permittedActions.Add(new PermittedActionDto
            {
                ActionName = "RegisterSeller",
                Description = "Register a new seller",
                Endpoint = "/api/company/sellers"
            });

            permittedActions.Add(new PermittedActionDto
            {
                ActionName = "ViewStatistics",
                Description = "View company statistics",
                Endpoint = "/api/company/statistics"
            });

            permittedActions.Add(new PermittedActionDto
            {
                ActionName = "ViewTransactions",
                Description = "View transaction summary",
                Endpoint = "/api/company/transactions"
            });

            permittedActions.Add(new PermittedActionDto
            {
                ActionName = "ViewStoresWithSellers",
                Description = "View company stores with sellers",
                Endpoint = "/api/companies/stores-with-sellers"
            });

            permittedActions.Add(new PermittedActionDto
            {
                ActionName = "ViewFiatTransactions",
                Description = "View fiat transaction summary",
                Endpoint = "/api/company/fiatTransactions"
            });
        }

        return permittedActions;
    }
    public async Task<bool> AppointSellerAsync(Guid companyId, AppointSellerDto appointSellerDto)
    {
        var store = await _dataService.Stores.GetByIdAsync(appointSellerDto.StoreId);
        if (store == null)
            return false;

        var seller = await _dataService.Users.GetByIdAsync(appointSellerDto.SellerId);
        if (seller == null)
            return false;

        seller.StoreId = store.Id;
        await _dataService.Users.UpdateAsync(seller);
        return true;
    }
    public async Task<bool> RegisterStore(StoreRegistrationDto storeDto)
    {
        // Validate company exists
        var company = await _dataService.Companies.GetByIdAsync(storeDto.CompanyId);
        if (company == null)
        {
            throw new ArgumentException(Res.Format("Error.CompanyNotFoundWithId", storeDto.CompanyId), nameof(storeDto.CompanyId));
        }

        // Check if company is active
        if (company.Status != CompanyStatus.Active)
        {
            throw new InvalidOperationException(Res.Format("Error.CompanyWithIdNotActive", storeDto.CompanyId));
        }
        // Create store
        var store = new StoreDto
        {
            Id = Guid.NewGuid(),
            FrontendId = _idGenerator.NewId(),
            UserName = storeDto.Name, // Use store name as username
            PasswordHash = string.Empty, // No password for stores initially
            CompanyId = storeDto.CompanyId,
            BusinessTypeId = storeDto.BusinessTypeId,
            CategoryId = storeDto.CategoryId,
            MallId = storeDto.MallId,
            Name = storeDto.Name,
            City = storeDto.City,
            Region = storeDto.Region,
            Address = storeDto.Address,
            Floor = storeDto.Floor,
            Row = storeDto.Row,
            Number = storeDto.Number,
            Email = storeDto.Email,
            WorkingHours = storeDto.WorkingHours,
            ContactPhone = storeDto.ContactPhone,
            Status = StoreStatus.PendingApproval, // New stores require admin approval
            TypeOfbusiness = GetBusinessTypeName(storeDto.BusinessTypeId) // Map BusinessTypeId to TypeOfbusiness string
        };

        var storeId = await _dataService.Stores.CreateAsync(store);

        var sellers = storeDto.Sellers ?? new List<CompanySellerDto>();
        if (sellers.Count > 0)
        {
            var existingIds = sellers.Where(s => s.UserId.HasValue)
                                     .Select(s => s.UserId!.Value)
                                     .Where(id => id != Guid.Empty)
                                     .Distinct()
                                     .ToList();

            List<UserDto> existingUsers;

            existingUsers = await _dataService.Users.GetByIdsAsync(existingIds);

            var existingById = existingUsers.ToDictionary(u => u.Id, u => u);

            foreach (var s in sellers)
            {
                if (s.UserId.HasValue)
                {
                    if (!existingById.TryGetValue(s.UserId.Value, out var user))
                        throw new ArgumentException(Res.Format("Error.UserNotFoundWithId", s.UserId), nameof(storeDto.Sellers));

                    if (user.Role != UserRole.Seller)
                        throw new InvalidOperationException(Res.Format("Error.UserNotSeller", user.Id));

                    if (user.CompanyId != store.CompanyId)
                        throw new InvalidOperationException(Res.Format("Error.UserNotInCompany", user.Id, store.CompanyId));


                    await _dataService.Users.UpdateStoreAssignmentAsync(user.Id, store.Id);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(s.Email))
                        throw new ArgumentException(Res.Format("Error.EmailRequiredForNewSeller"));

                    var registrationDto = new SellerRegistrationDto
                    {
                        Email = s.Email,
                        Username = string.IsNullOrWhiteSpace(s.UserName) ? s.Email : s.UserName,
                        StoreId = store.Id,
                        Role = UserRole.Seller,
                        DeviceToken = s.DeviceToken
                    };

                    await RegisterSeller(registrationDto, storeDto.CompanyId);
                }
            }
        }

        return true;
    }

    public async Task<PagedStoreStatisticsDto> GetStoreStatisticsAsync(Guid companyId, int page, int pageSize, DashBoardStoreStatisticsDto? filter = null)
    {
        var allItems = await _storeRepository.GetStoresByCompanyIdAsync(companyId);

        // Apply filter if provided
        if (filter != null)
        {
            // Фильтрация по строковому StoreId
            if (!string.IsNullOrWhiteSpace(Convert.ToString(filter.StoreId)))
            {
                allItems = allItems.Where(s => s.Id == filter.StoreId).ToList();
            }
        }

        var totalCount = allItems.Count();
        var pagedStores = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var items = new List<DashBoardStoreStatisticsDto>();
        foreach (var s in pagedStores)
        {
            var transactions = await _transactionRepository.GetTransactionsByStoreIdAsync(s.Id);
            var fiatTransactions = await _dataService.FiatTransactions.GetFiatTransactionsByStoreIdAsync(s.Id);

            items.Add(new DashBoardStoreStatisticsDto
            {
                StoreId = s.Id, 
                Date = DateTime.UtcNow,
                // ClientId = s.ClientId,
                OperationForBonusAccount = "Store Statistics",
                BonusGiven = transactions.Sum(t => t.BonusAmount),
                BonusGetting = transactions.Sum(t => t.TotalCost),
                SellingBonus = transactions.Sum(t => t.BonusAmount),
                Commission = transactions.Sum(t => t.CommissionPercent / 100m * t.TotalCost)
            });
        }

        return new PagedStoreStatisticsDto
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }

    public async Task<UserDto?> RegisterSeller(SellerRegistrationDto seller, Guid companyId)
    {
        // Validate company exists
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
            throw new ArgumentException(Res.Format("Error.CompanyNotFoundWithId", companyId), nameof(companyId));

        if (company.Status != CompanyStatus.Active)
            throw new InvalidOperationException(Res.Format("Error.CompanyWithIdNotActive", companyId));

        // Check email uniqueness
        var existingUser = await _userRepository.GetByEmailAsync(seller.Email);
        if (existingUser != null)
            throw new InvalidOperationException(Res.Format("Error.EmailAlreadyInUse", seller.Email));

        var verificationCode = new Random().Next(1000, 9999).ToString();

        var newSeller = new UserDto
        {
            Id = Guid.NewGuid(),
            FrontendId = _idGenerator.NewId(),
            Username = seller.Username,
            Email = seller.Email,
            PasswordHash = string.Empty,
            Role = UserRole.Seller,
            BonusBalance = 0,
            FiatBalance = 0,
            CompanyId = companyId,
            IsEmailVerified = true,
            VerificationCode = verificationCode,
            CreatedAt = DateTime.UtcNow,
            Phone = string.Empty,
            City = 0,
            INN = string.Empty,
            FirstName = string.Empty,
            LastName = string.Empty,
            Region = 0,
            DeviceToken = seller.DeviceToken,
            StoreId = seller.StoreId
        };
        
        await _userRepository.CreateAsync(newSeller);

        await _emailSender.SendEmailAsync(
            seller.Email,
            Res.Get("Email.SellerAccountReady.Subject"),
            Res.Format("Email.SellerAccountReady.Body", company.Name, verificationCode));

        return newSeller;
    }

    public async Task<DashboardStatisticsDto> GetStatisticsAsync(StatisticsQueryDto query)
    {
        // If companyId is provided, get stats for that company only
        if (query.CompanyId.HasValue)
        {
            var company = await _companyRepository.GetByIdAsync(query.CompanyId.Value);
            if (company == null)
            {
                throw new ArgumentException($"Company with ID {query.CompanyId} not found", nameof(query.CompanyId));
            }

            // Get stores for this company
            var stores = await _storeRepository.GetStoresByCompanyIdAsync(query.CompanyId.Value);

            // Get transactions for this company
            var transactions = await _transactionRepository.GetTransactionsByCompanyIdAsync(query.CompanyId.Value);

            // Get fiat transactions for this company
            var fiatTransactions = await _dataService.FiatTransactions.GetFiatTransactionsByCompanyIdAsync(query.CompanyId.Value);

            // Filter by date range if provided
            if (query.StartDate.HasValue && query.EndDate.HasValue)
            {
                transactions = transactions.Where(t =>
                    t.Timestamp >= query.StartDate.Value &&
                    t.Timestamp <= query.EndDate.Value).ToList();

                fiatTransactions = fiatTransactions.Where(t =>
                    t.Timestamp >= query.StartDate.Value &&
                    t.Timestamp <= query.EndDate.Value).ToList();
            }

            return new DashboardStatisticsDto
            {
                TotalBonusCirculation = company.OriginalBonusBalance,
                CurrentActiveBonus = company.BonusBalance,
                CurrentActiveFiat = company.FiatBalance,
                TotalTransactions = transactions.Count(),
                TotalFiatTransactions = fiatTransactions.Count(),
                ActiveUsers = 0, // Not applicable for a single company
                ActiveCompanies = 1,
                ActiveStores = stores.Count(s => s.Status == StoreStatus.Active)
            };
        }
        else
        {
            // Get system-wide statistics
            decimal totalCirculation = await _transactionRepository.GetTotalBonusCirculationAsync();
            decimal activeBonus = await _transactionRepository.GetTotalActiveBonus();
            int transactionCount = await _transactionRepository.GetTotalTransactionsCountAsync();

            // Get active companies
            var activeCompanies = await _companyRepository.GetCompaniesByStatusAsync(CompanyStatus.Active);

            // Get active buyer users
            var activeUsers = (await _userRepository.GetUsersByRoleAsync(UserRole.Buyer)).Count();

            // Get all active stores
            var allStores = await _storeRepository.GetAllAsync();
            var activeStores = allStores.Count(s => s.Status == StoreStatus.Active);

            return new DashboardStatisticsDto
            {
                TotalBonusCirculation = totalCirculation,
                CurrentActiveBonus = activeBonus,
                TotalTransactions = transactionCount,
                ActiveUsers = activeUsers,
                ActiveCompanies = activeCompanies.Count(),
                ActiveStores = activeStores
            };
        }
    }
    
    public async Task<StoresWithSellersPagedResponseDto> GetStoresWithSellersAsync(Guid companyId, StoresFilterRequestDto filter)
    {
        // Validate company exists
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
        {
            throw new ArgumentException(Res.Format("Error.CompanyNotFoundWithId", companyId), nameof(companyId));
        }

        // Validate filter parameters
        if (filter.StoreStatus.HasValue)
        {
            if (!Enum.IsDefined(typeof(StoreStatus), filter.StoreStatus.Value))
            {
                    throw new ArgumentException(Res.Format("Error.InvalidStoreStatusValue", filter.StoreStatus.Value), nameof(filter.StoreStatus));
            }
        }

        if (filter.SellerRole.HasValue)
        {
            if (!Enum.IsDefined(typeof(UserRole), filter.SellerRole.Value))
            {
                    throw new ArgumentException(Res.Format("Error.InvalidSellerRoleValue", filter.SellerRole.Value), nameof(filter.SellerRole));
            }

            // Ensure we're only filtering for Seller role
            if (filter.SellerRole.Value != UserRole.Seller)
            {
                filter = filter with { SellerRole = UserRole.Seller };
            }
        }

        // Get all stores for this company
        var allStores = await _storeRepository.GetStoresByCompanyIdAsync(companyId);

        // Apply store status filter if provided
        if (filter.StoreStatus.HasValue)
        {
            allStores = allStores.Where(s => s.Status == filter.StoreStatus.Value).ToList();
        }

        // Calculate total count before pagination
        var totalCount = allStores.Count();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        // Apply pagination
        var paginatedStores = allStores
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        // Create list to hold stores with sellers
        var storesWithSellers = new List<StoreWithSellersDto>();

        // Get all sellers for all paginated stores in a single operation to avoid N+1 queries
        var storeIds = paginatedStores.Select(s => s.Id).ToList();
        Dictionary<Guid, List<UserDto>> sellersByStore = new();

        // Prepare a dictionary to hold sellers by store ID
        foreach (var storeId in storeIds)
        {
            var storeSellers = await _storeRepository.GetSellersByStoreIdAsync(storeId);

            // We know sellers should have Seller role, but double-check for safety
            sellersByStore[storeId] = storeSellers.Where(s => s.Role == UserRole.Seller).ToList();
        }

        // For each store, add it with its sellers to the result
        foreach (var store in paginatedStores)
        {
            if (!sellersByStore.TryGetValue(store.Id, out var sellers))
            {
                sellers = new List<UserDto>();
            }

            storesWithSellers.Add(new StoreWithSellersDto
            {
                Id = store.Id,
                CompanyId = store.CompanyId,
                BusinessTypeId = store.BusinessTypeId,
                CategoryId = store.CategoryId,
                MallId = store.MallId,
                Name = store.Name,
                City = store.City,
                Region = store.Region,
                Floor = store.Floor,
                Row = store.Row,
                Number = store.Number,
                Email = store.Email,
                WorkingHours = store.WorkingHours,
                ContactPhone = store.ContactPhone,
                Status = store.Status,
                SellerIds = sellers.Select(s => s.Id).ToList()
            });
        }

        // Create and return the paginated response
        return new StoresWithSellersPagedResponseDto
        {
            Stores = storesWithSellers,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }
    public async Task<CompanyDto?> FindContractAsync(FindCompanyDto findCompanyDto)
    {
        if (string.IsNullOrWhiteSpace(findCompanyDto.BIN) &&
            string.IsNullOrWhiteSpace(findCompanyDto.Number_of_contract) &&
            string.IsNullOrWhiteSpace(findCompanyDto.INN) &&
            string.IsNullOrWhiteSpace(findCompanyDto.Name))
        {
            throw new ArgumentException(Res.Get("Error.AtLeastOneParameterRequired"));
        }

        return await _companyRepository.FindContractAsync(
            findCompanyDto.BIN,
            findCompanyDto.Number_of_contract,
            findCompanyDto.INN,
            findCompanyDto.Name
        );
    }

    // public async Task<DashBoardStoreStatisticsDto> GetStoreStatisticsAsync(Guid storeId)
    // {
    //     var store = await _userRepository.GetByIdAsync(storeId);
    //     if (store == null)
    //     {
    //         throw new ArgumentException($"Store with ID {storeId} not found", nameof(storeId));
    //     }
    //     var transactions = await _transactionRepository.GetTransactionsByStoreIdAsync(storeId);
    //     var fiatTransactions = await _dataService.FiatTransactions.GetFiatTransactionsByStoreIdAsync(storeId);

    //     return new DashBoardStoreStatisticsDto
    //     {
    //         StoreId = storeId,
    //         Date = DateTime.UtcNow,
    //         // ClientId = store.CompanyId,
    //         OperationForBonusAccount = "Store Statistics",
    //         BonusGiven = transactions.Sum(t => t.BonusAmount),
    //         BonusGetting = transactions.Sum(t => t.TotalCost),
    //         SellingBonus = transactions.Sum(t => t.BonusAmount),
    //         Commission = transactions.Sum(t => t.CommissionPercent) / 100 * transactions.Sum(t => t.TotalCost)
    //     };
    // }
    public async Task<FiatTransactionDto> GetFiatTransactionSummaryAsync(Guid? companyId)
    {
        if (!companyId.HasValue)
        {
            throw new ArgumentNullException(nameof(companyId), "Company ID must be provided");
        }

        // Validate company exists
        var company = await _companyRepository.GetByIdAsync(companyId.Value);
        if (company == null)
        {
            throw new ArgumentException($"Company with ID {companyId} not found", nameof(companyId));
        }

        // Get most recent transaction for this company
        var fiatTransactions = await _dataService.FiatTransactions.GetFiatTransactionsByCompanyIdAsync(companyId.Value);
        var latestTransaction = fiatTransactions.OrderByDescending(t => t.Timestamp).FirstOrDefault();

        if (latestTransaction == null)
        {
            // If no transactions exist, return a placeholder
            return new FiatTransactionDto
            {
                Id = Guid.Empty,
                CompanyId = companyId,
                BonusAmount = 0,
                TotalCost = 0,
                Status = FiatTransactionStatus.Failed,
                Timestamp = DateTime.UtcNow,
                Description = "No transactions found"
            };
        }

        return latestTransaction;
    }
    public async Task<bool> UpdateStoreAsync(Guid storeId, StoreUpdateDto storeUpdate)
    {
        var store = await _storeRepository.GetByIdAsync(storeId);
        if (store == null)
        {
            _logger.LogError($"Store with ID {storeId} not found");
            return false;
        }

        var updatedStore = store with
        {
            Name = storeUpdate.Name,
            City = storeUpdate.City,
            Region = storeUpdate.Region,
            MallId = storeUpdate.Mall,
            Floor = storeUpdate.Floor,
            Row = storeUpdate.Row,
            Number = storeUpdate.Number,
            Email = storeUpdate.Email,
            WorkingHours = storeUpdate.WorkingHours,
            ContactPhone = storeUpdate.ContactPhone,
        };

        await _storeRepository.UpdateAsync(updatedStore);
        return true;
    }

    public async Task<bool> DeleteStoreAsync(Guid storeId)
    {
        return await _storeRepository.UpdateStatusAsync(storeId, StoreStatus.Inactive);
    }
    // public async Task<bool> UpdateSeller(Guid sellerId, SellerUpdateDto sellerUpdate)
    // {
    //     var seller = await _sellerRepository.GetByIdAsync(sellerId);
    //     if (seller == null)
    //     {
    //         _logger.LogError($"Seller with ID {sellerId} not found");
    //         return false;
    //     }

    //     var updatedSeller = seller with
    //     {
    //         Name = sellerUpdate.Name,
    //         Phone = sellerUpdate.Phone,
    //         BIK = sellerUpdate.BIK,
    //         Address = sellerUpdate.Address,
    //     };

    //     await _sellerRepository.UpdateAsync(updatedSeller);
    //     return true;
    // }
    public async Task<bool> DeleteSellerAsync(Guid sellerId)
    {
        var seller = await _userRepository.GetByIdAsync(sellerId);
        if (seller == null)
        {
            _logger.LogError($"Seller with ID {sellerId} not found");
            return false;
        }

        await _userRepository.DeleteAsync(sellerId);
        return true;
    }
    public async Task<bool> UpdateCompanyAsync(Guid companyId, CompanyUpdateDto companyUpdate)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
        {
            _logger.LogError($"Company with ID {companyId} not found");
            return false;
        }

        var updatedCompany = company with
        {
            Name = companyUpdate.Name,
            City = companyUpdate.City,
            Region = companyUpdate.Region,
            ContactPhone = companyUpdate.ContactPhone,
            ContactEmail = companyUpdate.ContactEmail
            // Status = companyUpdate.Status
        };

        await _companyRepository.UpdateAsync(updatedCompany);
        return true;
    }
    public async Task<bool> DeleteCompanyAsync(Guid companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
        {
            _logger.LogError($"Company with ID {companyId} not found");
            return false;
        }

        await _companyRepository.DeleteAsync(companyId);
        return true;
    }

    private string GetBusinessTypeName(int businessTypeId)
    {
        return businessTypeId switch
        {
            1 => "Shopping",
            2 => "Beauty", 
            3 => "Health",
            4 => "Food",
            _ => "Unknown"
        };
    }

    // Real-time monitoring methods implementation
    public async Task<CompanyRealTimeStatisticsDto> GetRealTimeStatisticsAsync(Guid companyId)
    {
        return await _realTimeMonitoringService.GetCurrentStatisticsAsync(companyId);
    }

    public async Task<CompanyDailyStatisticsDto> GetDailyStatisticsAsync(Guid companyId, DateTime date)
    {
        // Create daily statistics by aggregating real-time data
        var baseStats = await _realTimeMonitoringService.GetStatisticsForDateAsync(companyId, date);
        var stores = await _storeRepository.GetStoresByCompanyIdAsync(companyId);
        
        var storeStats = new List<StoreDailyStatisticsDto>();
        
        foreach (var store in stores)
        {
            var storeTransactions = (await _transactionRepository.GetTransactionsByStoreIdAsync(store.Id))
                .Where(t => t.Timestamp.Date == date.Date)
                .ToList();

            var storeSalesAmount = storeTransactions.Sum(t => t.TotalCost);
            var storeDealCount = storeTransactions.Count;
            var storeRefundCount = storeTransactions.Count(t => t.Status == Shared.Models.TransactionStatus.Reversed);
            var storeCommission = storeTransactions.Sum(t => (t.CommissionPercent / 100m) * t.TotalCost);

            storeStats.Add(new StoreDailyStatisticsDto
            {
                StoreId = store.Id,
                StoreName = store.Name,
                SalesAmount = Math.Round(storeSalesAmount, 2),
                DealCount = storeDealCount,
                RefundCount = storeRefundCount,
                Commission = Math.Round(storeCommission, 2)
            });
        }

        return new CompanyDailyStatisticsDto
        {
            CompanyId = companyId,
            Date = date,
            BonusBalance = baseStats.BonusBalance,
            WalletBalance = baseStats.WalletBalance,
            SalesAmount = baseStats.SalesAmount,
            DealCount = baseStats.DealCount,
            RefundCount = baseStats.RefundCount,
            CashbackAmount = baseStats.CashbackAmount,
            Commission = baseStats.Commission,
            CommissionPaymentStatus = baseStats.CommissionPaymentStatus,
            StoreStatistics = storeStats
        };
    }

    public async Task<CompanyQuarterlyStatisticsDto> GetQuarterlyStatisticsAsync(Guid companyId, int year, int quarter)
    {
        return await _realTimeMonitoringService.GetQuarterlyStatisticsAsync(companyId, year, quarter);
    }

    public async Task<List<CompanyDailyStatisticsDto>> GetMonthlyStatisticsAsync(Guid companyId, int year, int month)
    {
        return await _realTimeMonitoringService.GetMonthlyStatisticsAsync(companyId, year, month);
    }

    public async Task<CompanyRealTimeStatisticsDto> GetStatisticsAtDateAsync(Guid companyId, DateTime date)
    {
        return await _realTimeMonitoringService.GetStatisticsAtDateAsync(companyId, date);
    }

    public async Task<TransactionDto> GetTransactionSummaryAsync(Guid? companyId)
    {
        return await GetLastTransactionAsync(companyId);
    }
}
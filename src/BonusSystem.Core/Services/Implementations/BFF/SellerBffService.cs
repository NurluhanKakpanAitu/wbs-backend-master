using System.Collections;
using BonusSystem.Application.Common.Transactions;
using BonusSystem.Core.Repositories; 
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.Extensions.Logging;
using BonusSystem.Localization;

namespace BonusSystem.Core.Services.Implementations.BFF;

/// <summary>
/// BFF service for Seller role
/// </summary>
public class SellerBffService : BaseBffService, ISellerBffService
{
    private readonly ILogger<SellerBffService> _logger;
    private readonly ITransactionExecutor _executor;
    private readonly ICommissionBffService _commissionBffService;
    private readonly ICompanyRepository _companyRepository;
    private readonly IFirebasePushNotificationService _pushNotificationService; 
    public SellerBffService(
        IDataService dataService,
        IAuthenticationService authService,
        ILogger<SellerBffService> logger,
        ITransactionExecutor executor,
        ICommissionBffService commissionBffService,
        ICompanyRepository companyRepository,
        IFirebasePushNotificationService pushNotificationService)
        : base(dataService, authService)
    {
        _logger = logger;
        _executor = executor;
        _commissionBffService = commissionBffService;
        _companyRepository = companyRepository;
        _pushNotificationService = pushNotificationService;
    }

    /// <summary>
    /// Gets the permitted actions for a seller
    /// </summary>
    public override async Task<IEnumerable<PermittedActionDto>> GetPermittedActionsAsync(Guid userId)
    {
        var role = await _dataService.Users.GetUserRoleAsync(userId);
        if (role != UserRole.Seller)
        {
            return Enumerable.Empty<PermittedActionDto>();
        }

        return new List<PermittedActionDto>
        {
            new() { ActionName = "ProcessTransaction", Description = "Process transaction", Endpoint = "/api/sellers/transactions" },
            new() { ActionName = "ProcessFiatTransaction", Description = "Process fiat transaction", Endpoint = "/api/sellers/fiatTransactions"},
            new() { ActionName = "ProcessCashBackTransaction", Description = "Process cashback transaction", Endpoint = "/api/sellers/cashbackTransactions"},
            new() { ActionName = "GetBuyerBalance", Description = "Get buyer's bonus balance", Endpoint = "/api/sellers/buyers/{id}/balance" },
            new() { ActionName = "GetStoreBalance", Description = "Get store's bonus balance", Endpoint = "/api/sellers/stores/{id}/balance" },
            new() { ActionName = "GetTransactions", Description = "Get store transactions", Endpoint = "/api/sellers/stores/{id}/transactions" },
            new() { ActionName = "RequestTransactionReturnAsync", Description = "Request return of the transaction", Endpoint = "/api/sellers/transactionReturns/return"},
            new() { ActionName = "GetTransactionReturnsRequestedBySeller",Description="Get seller requested transaction returns", Endpoint="/api/sellers/transactionsReturns/list"},
            new() { ActionName = "GetFiatTransactions", Description = "Get store fiat transactions", Endpoint = "/api/sellers/stores/{id}/fiatTransactions" },
        };
    }
    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId)
    {
        var notifications = await _dataService.Notifications.GetUserNotificationsAsync(userId);
        return notifications;
    }
    public async Task<PagedResult<CombinedTransactionDto>> GetBuyerTransactionsByFrontendIdAsync(Guid sellerId, string frontendId, int page, int pageSize)
    {
        var user = await _dataService.Users.GetByFrontendIdAsync(frontendId);
        if (user == null)
        {
            return null;
        }
        var seller = await _dataService.Users.GetByIdAsync(sellerId);
        if (seller == null)
        {
            return null;
        }
        
        return await _dataService.Users.AllTransactionsForUserAsync(user.Id, page, pageSize);
    }
    public async Task<CompanyDto> GetCompanyForSeller(Guid companyId)
    {
        var company = await _dataService.Companies.GetByIdAsync(companyId);
        return company;
    } 
    
    /// <summary>
    /// Processes a transaction
    /// </summary>
    public async Task<TransactionResultDto> ProcessTransactionAsync(Guid sellerId, TransactionRequestDto request)
    {
        TransactionDto? transactionDto = null;
        UserDto? buyer = null;
        StoreDto? store = null;

        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var (seller, buyerDto, storeDto, company) = await ValidateTransactionParticipants(sellerId, request.BuyerId);
            buyer = buyerDto;
            store = storeDto;

            if (company is null)
                throw new InvalidOperationException(Res.Get("Error.CompanyNotFound"));

            if (request.Type == TransactionType.Spend && buyer.BonusBalance < request.BonusAmount)
                throw new InvalidOperationException(Res.Get("Error.InsufficientBonusBalance"));

            if (request.Type == TransactionType.Earn && company.BonusBalance < request.BonusAmount)
                throw new InvalidOperationException(Res.Get("Error.InsufficientCompanyBonusBalance"));

            if (request.BonusAmount > 0)
                transactionDto = await CreateBonusTransactionAsync(store, request);

            await UpdateTransactionBalances(buyer, company, request);
        });

        if (transactionDto != null && buyer != null && !string.IsNullOrEmpty(buyer.DeviceToken))
        {
            try
            {
                 var notificationData = new Dictionary<string, string>
                 {
                     { "action", "transaction_pending" },
                     { "transactionId", transactionDto.Id.ToString() },
                     { "transactionType", request.Type.ToString() },
                     { "bonusAmount", request.BonusAmount.ToString() },
                     { "totalCost", request.TotalCost.ToString()   },
                     { "storeName", store?.Name ?? Res.Get("Common.UnknownStore") },
                     { "timestamp", DateTime.UtcNow.ToString("O") }
                 };

                await _pushNotificationService.SendDataOnlyMessageAsync(buyer.DeviceToken, notificationData);
                _logger.LogInformation("Silent notification sent to buyer {BuyerId} for transaction {TransactionId}", buyer.Id, transactionDto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send silent notification to buyer {BuyerId} for transaction {TransactionId}", buyer.Id, transactionDto.Id);
            }
        }

        return new TransactionResultDto
        {
            Success = true,
            Transaction = transactionDto,
        };
    }
    /// <summary>
    /// Processes a fiat transaction
    /// </summary>
    public async Task<FiatTransactionResultDto> ProcessFiatTransactionAsync(Guid sellerId, FiatTransactionRequestDto request)
    {
        FiatTransactionDto? fiatTransactionDto = null;
        UserDto? buyer = null;
        StoreDto? store = null;

        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var (fiatTransactionAmount, fiatCashBack) = CalculateFiatValues(request);
            var (seller, buyerDto, storeDto, company) = await ValidateTransactionParticipants(sellerId, request.BuyerId);
            buyer = buyerDto;
            store = storeDto;

            if (company is null)
                throw new InvalidOperationException(Res.Get("Error.CompanyNotFound"));

            if (buyer.FiatBalance < fiatTransactionAmount)
                throw new BusinessException(Res.Get("Error.InsufficientFiatBalance"));

            if (fiatCashBack > 0 && company.FiatBalance < fiatCashBack)
                throw new BusinessException(Res.Get("Error.InsufficientCompanyFiatBalanceForCashback"));

            fiatTransactionDto = await CreateFiatTransactionAsync(sellerId, store, request);

            await UpdateFiatTransactionBalances(buyer, company, request);

        });

        // Send silent notification to buyer after successful fiat transaction
        if (fiatTransactionDto != null && buyer != null && !string.IsNullOrEmpty(buyer.DeviceToken))
        {
            try
            {
                 var notificationData = new Dictionary<string, string>
                 {
                     { "action", "pending_fiat_transaction" },
                     { "transactionId", fiatTransactionDto.Id.ToString() },
                     { "totalCost", request.TotalCost.ToString() },
                     { "fiatCashbackRate", request.FiatCashbackRate?.ToString() ?? "0" },
                     { "storeName", store?.Name ?? Res.Get("Common.UnknownStore") },
                     { "timestamp", DateTime.UtcNow.ToString("O") }
                 };

                await _pushNotificationService.SendDataOnlyMessageAsync(buyer.DeviceToken, notificationData);
                _logger.LogInformation("Silent notification sent to buyer {BuyerId} for fiat transaction {TransactionId}", buyer.Id, fiatTransactionDto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send silent notification to buyer {BuyerId} for fiat transaction {TransactionId}", buyer.Id, fiatTransactionDto.Id);
            }
        }

        return new FiatTransactionResultDto
        {
            Success = true,
            FiatTransaction = fiatTransactionDto,
        };
    }

    /// <summary>
    /// Processes a combined transaction (bonuses + fiat payment)
    /// </summary>
    public async Task<CombinedTransactionResultDto> ProcessCombinedTransactionAsync(Guid sellerId, CombinedTransactionRequestDto request)
    {
        TransactionDto? bonusTransactionDto = null;
        FiatTransactionDto? fiatTransactionDto = null;
        UserDto? buyer = null;
        StoreDto? store = null;

        // Get default commission percentage from service
        decimal commissionPercent = _commissionBffService.GetDefaultCommissionPercent();

        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var (seller, buyerDto, storeDto, company) = await ValidateTransactionParticipants(sellerId, request.BuyerId);
            buyer = buyerDto;
            store = storeDto;

            if (company is null)
                throw new InvalidOperationException(Res.Get("Error.CompanyNotFound"));

            decimal bonusAmount = request.TotalCost * (request.BonusPercent / 100m);

            decimal cashbackAmount = (request.TotalCost - bonusAmount) * (request.CashbackPercent / 100m); 

            decimal paymentAmount = request.TotalCost - bonusAmount;

            if (request.DeductBonuses && buyer.BonusBalance < bonusAmount)
                throw new InvalidOperationException(Res.Get("Error.InsufficientBonusBalance"));

            if (request.CashbackPercent > 0 && buyer.FiatBalance < paymentAmount)
                throw new BusinessException(Res.Get("Error.InsufficientFiatBalance"));

            if (request.CashbackPercent > 0 && company.FiatBalance < cashbackAmount)
                throw new BusinessException(Res.Get("Error.InsufficientCompanyFiatBalanceForCashback"));

            // Note: Commission is deducted from the payment amount, not from company balance
            if (request.DeductBonuses && bonusAmount > 0)
            {
                var bonusRequest = new TransactionRequestDto
                {
                    BuyerId = request.BuyerId,
                    BonusAmount = bonusAmount,
                    TotalCost = request.TotalCost,
                    Type = TransactionType.Spend,
                    CommissionPercent = commissionPercent,
                    SellerId = sellerId
                };
                bonusTransactionDto = await CreateBonusTransactionAsync(store, bonusRequest);
            }
            // создать другой сценарий если кэшбэк 0  
            // не волнует нас фиатный баланс компании
            if (request.CashbackPercent > 0 && paymentAmount > 0 && bonusTransactionDto != null)
            {
                var fiatRequest = new FiatTransactionRequestDto
                {
                    BuyerId = request.BuyerId,
                    BonusAmount = bonusAmount,
                    TotalCost = request.TotalCost,
                    FiatCashbackRate = request.CashbackPercent,
                    CommissionPercent = commissionPercent,
                    RelatedBonusTransactionId = bonusTransactionDto.Id
                };
                fiatTransactionDto = await CreateFiatTransactionAsync(sellerId, store, fiatRequest);
            }

            // Update balances
            if (request.DeductBonuses && bonusAmount > 0)
            {
                var bonusRequest = new TransactionRequestDto
                {
                    BuyerId = request.BuyerId,
                    BonusAmount = bonusAmount,
                    TotalCost = request.TotalCost,
                    Type = TransactionType.Spend,
                    CommissionPercent = commissionPercent
                };
                await UpdateTransactionBalances(buyer, company, bonusRequest);
            }

            if (request.CashbackPercent > 0 && paymentAmount > 0)
            {
                var fiatRequest = new FiatTransactionRequestDto
                {
                    BuyerId = request.BuyerId,
                    BonusAmount = 0,
                    TotalCost = paymentAmount,
                    FiatCashbackRate = request.CashbackPercent / 100m,
                    CommissionPercent = commissionPercent
                };
                await UpdateFiatTransactionBalances(buyer, company, fiatRequest);
            }
        });

        if (buyer != null && !string.IsNullOrEmpty(buyer.DeviceToken))
        {
            try
            {
                var notificationData = new Dictionary<string, string>
                {
                    { "action", "combined_transaction_completed" },
                    { "transactionId", (bonusTransactionDto?.Id ?? fiatTransactionDto?.Id)?.ToString() ?? "" },
                    { "totalCost", request.TotalCost.ToString() },
                    { "bonusAmount", (request.TotalCost * (request.BonusPercent / 100m)).ToString() },
                    { "paymentAmount", (request.TotalCost - (request.TotalCost * (request.BonusPercent / 100m))).ToString() },
                    { "cashbackPercent", request.CashbackPercent.ToString() },
                    { "commissionPercent", commissionPercent.ToString() },
                    { "storeName", store?.Name ?? Res.Get("Common.UnknownStore") },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                };

                await _pushNotificationService.SendDataOnlyMessageAsync(buyer.DeviceToken, notificationData);
                _logger.LogInformation("Silent notification sent to buyer {BuyerId} for combined transaction", buyer.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send silent notification to buyer {BuyerId} for combined transaction", buyer.Id);
            }
        }

        var response = new CombinedTransactionResponseDto
        {
            Id = bonusTransactionDto?.Id ?? fiatTransactionDto?.Id ?? Guid.NewGuid(),
            FrontendId = fiatTransactionDto?.FrontendId ?? string.Empty,
            PaymentAmount = request.TotalCost - (request.TotalCost * (request.BonusPercent / 100m)),
            Amount = request.TotalCost,
            BonusPercent = request.BonusPercent,
            CashbackPercent = request.CashbackPercent,
            CommissionPercent = commissionPercent,
            BonusAmount = request.TotalCost * (request.BonusPercent / 100m),
            CashbackAmount = request.TotalCost * (request.CashbackPercent / 100m),
            CommissionAmount = request.TotalCost * (commissionPercent / 100m),
            Timestamp = DateTime.UtcNow,
            Type = request.DeductBonuses ? 0 : 2, 
            Status = 0 
        };

        return new CombinedTransactionResultDto
        {
            Success = true,
            Transaction = response
        };
    }



    public async Task<PagedResult<CombinedTransactionDto>> GetTransactionsForSeller(Guid sellerId, int page, int pageSize)
    {
        var user = await _dataService.Users.GetByIdAsync(sellerId);
        if (user == null)
        {
            return null;
        }
        return await _dataService.Users.AllTransactionsForUserAsync(user.Id, page, pageSize);
    }
    public async Task<FiatTransactionResultDto> ProcessCashBackTransactionAsync(FiatCashbackRequestDto request)
    {
        FiatTransactionDto? fiatTransactionDto = null;

        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var (seller, buyer, store, company) = await ValidateTransactionParticipants(request.SellerId, request.BuyerId);

            if (request.FiatTransaction == null)
                throw new BusinessException(Res.Get("Error.InvalidFiatTransaction"));

            await UpdateCashBackBalances(buyer, company, request.FiatTransaction);


        });
        return new FiatTransactionResultDto
        {
            Success = true,
            FiatTransaction = fiatTransactionDto,

        };
    }

    /// <summary>
    /// Creates a pending transaction return request for either a bonus or fiat transaction.
    /// </summary>
    public async Task<TransactionReturnResultDto> RequestTransactionReturnAsync(Guid sellerId, TransactionReturnRequestDto request)
    {
        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var seller = await _dataService.Users.GetByIdAsync(sellerId);
            if (seller == null || seller.Role != UserRole.Seller)
                throw new InvalidOperationException(Res.Get("Error.InvalidSeller"));

            var returnRequest = new TransactionReturnDto
            {
                Id = Guid.NewGuid(),
                Reason = request.Reason,
                Status = TransactionReturnStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                RequestedByUserId = sellerId
            };

            if (request.BonusTransactionId.HasValue)
            {
                var transaction = await _dataService.Transactions.GetByIdAsync(request.BonusTransactionId.Value);
                if (transaction == null || transaction.Status != TransactionStatus.Completed)
                    throw new InvalidOperationException(Res.Get("Error.InvalidOrNonCompletedBonusTransaction"));
                returnRequest.BonusTransactionId = transaction.Id;
            }
            else if (request.FiatTransactionId.HasValue)
            {
                var transaction = await _dataService.FiatTransactions.GetByIdAsync(request.FiatTransactionId.Value);
                if (transaction == null || transaction.Status != FiatTransactionStatus.Completed)
                    throw new InvalidOperationException(Res.Get("Error.InvalidOrNonCompletedFiatTransaction"));
                returnRequest.FiatTransactionId = transaction.Id;
            }
            else
            {
                throw new InvalidOperationException(Res.Get("Error.MustProvideBonusOrFiatTransactionId"));
            }

            await _dataService.TransactionReturns.CreateAsync(returnRequest);
        });

        return new TransactionReturnResultDto { Success = true };
    }

    /// <summary>
    /// Retrieves all transaction returns requested by the given seller.
    /// </summary>
    public async Task<IEnumerable<TransactionReturnDto>> GetTransactionReturnsRequestedBySellerAsync(Guid sellerId)
    {
        var seller = await _dataService.Users.GetByIdAsync(sellerId);
        if (seller == null || seller.Role != UserRole.Seller)
            return Enumerable.Empty<TransactionReturnDto>();

        var returns = await _dataService.TransactionReturns.GetByRequestedUserIdAsync(sellerId, null);

        return returns.OrderByDescending(r => r.RequestedAt);
    }

    /// <summary>
    /// Gets the bonus balance for a buyer
    /// </summary> 
    /// 
    
    public async Task<BonusBalanceDto?> GetBuyerBonusBalanceAsync(string buyerId)
    {
        var buyer = await _dataService.Users.GetByFrontendIdAsync(buyerId);
        if (buyer is null)
        {
            return null;
        }

        return new BonusBalanceDto
        {
            UserId = buyer.Id,
            FrontendId = buyer.FrontendId,
            BonusBalance = buyer.BonusBalance,
            FiatBalance = buyer.FiatBalance
        };
    }

    /// <summary>
    /// Gets the bonus balance for a store
    /// </summary>
    public async Task<decimal> GetStoreBonusBalanceAsync(Guid storeId)
    {
        return await _dataService.Stores.GetStoreBonusBalanceAsync(storeId);
    }

    public async Task<decimal> GetStoreFiatBalanceAsync(Guid storeId)
    {
        return await _dataService.Stores.GetStoreFiatBalanceAsync(storeId);
    }
    public async Task<decimal> GetStoreBonusBalanceByUserIdAsync(Guid userId)
    {
        var storeBySeller = await _dataService.Stores.GetStoreBySellerIdAsync(userId);

        return await GetStoreBonusBalanceAsync(storeBySeller.Id);
    }

    /// <summary>
    /// Gets the bonus transactions for a store
    /// </summary>
    public async Task<IEnumerable<StoreBonusTransactionsDto>> GetStoreBonusTransactionsAsync(Guid storeId)
    {
        var store = await _dataService.Stores.GetByIdAsync(storeId);
        if (store == null)
        {
            return [];
        }

        var transactions = await _dataService.Transactions.GetTransactionsByStoreIdAsync(storeId);
        var totalAmount = transactions.Sum(t => t.BonusAmount);

        var result = new StoreBonusTransactionsDto
        {
            StoreId = storeId,
            StoreName = store.Name,
            TotalTransactions = totalAmount,
            Transactions = transactions.OrderByDescending(t => t.Timestamp).ToList()
        };

        return [result];
    }
        /// <summary>
    /// Gets the fiat transactions for a store
    /// </summary>
    public async Task<IEnumerable<StoreFiatTransactionsDto>> GetStoreFiatTransactionsAsync(Guid storeId)
    {
        var store = await _dataService.Stores.GetByIdAsync(storeId);
        if (store == null)
        {
            return [];
        }

        var fiatTransactions = await _dataService.FiatTransactions.GetFiatTransactionsByStoreIdAsync(storeId);
        var totalAmount = fiatTransactions.Sum(t => t.FiatTransactionAmount);

        var result = new StoreFiatTransactionsDto
        {
            StoreId = storeId,
            StoreName = store.Name,
            TotalFiatTransactions = totalAmount,
            FiatTransactions = fiatTransactions.OrderByDescending(t => t.Timestamp).ToList()
        };

        return [result];
    }

    public async Task<IEnumerable<StoreBonusTransactionsDto>> GetStoreBonusTransactionsByUserIdAsync(Guid userId)
    {
        var store = await _dataService.Stores.GetStoreBySellerIdAsync(userId);
        if (store == null)
        {
            return [];
        }

        return await GetStoreBonusTransactionsAsync(store.Id);
    }
    public async Task<IEnumerable<StoreFiatTransactionsDto>> GetStoreFiatTransactionsByUserIdAsync(Guid userId)
    {
        var store = await _dataService.Stores.GetStoreBySellerIdAsync(userId);
        if (store == null)
        {
            return [];
        }

        return await GetStoreFiatTransactionsAsync(store.Id);
    }
    
    private async Task<(UserDto seller, UserDto buyer, StoreDto store, CompanyDto? company)> ValidateTransactionParticipants(Guid sellerId, Guid buyerId)
    {
        if (sellerId == buyerId)
        {
            _logger.LogWarning("A seller attempted to perform a transaction with themselves. SellerId: {SellerId}", sellerId);
            throw new InvalidOperationException(Res.Get("Error.SellerCannotTransactWithSelf"));
        }

        var sellerDto = await _dataService.Users.GetByIdAsync(sellerId);
        var buyerDto = await _dataService.Users.GetByIdAsync(buyerId);

        if (sellerDto == null || sellerDto.Role != UserRole.Seller)
        {
            _logger.LogWarning("Seller not found or has incorrect role for ID {SellerId}", sellerId);
            throw new ArgumentException(Res.Get("Error.SellerNotFoundOrInvalidRole"));
        }

        if (buyerDto == null)
        {
            _logger.LogWarning("Buyer not found for ID {BuyerId}", buyerId);
            throw new ArgumentException(Res.Get("Error.BuyerNotFound"));
        }

        if (sellerDto.StoreId == null)
        {
            _logger.LogError("Seller not linked to a store: {SellerId}", sellerId);
            throw new InvalidOperationException(Res.Get("Error.SellerNotLinkedToStore"));
        }
        
        var storeDto = await _dataService.Stores.GetByIdAsync(sellerDto.StoreId.Value);

        if (storeDto == null)
        {
            _logger.LogError("Store with ID {StoreId} not found for seller {SellerId}", sellerDto.StoreId.Value, sellerId);
            throw new InvalidOperationException(Res.Get("Error.StoreNotFoundForSeller"));
        }
        
        var companyDto = await _companyRepository.GetByIdAsync(storeDto.CompanyId);

        return (sellerDto, buyerDto, storeDto, companyDto);
    }

    private (decimal, decimal) CalculateFiatValues(FiatTransactionRequestDto request)
    {
        // Логика расчета
        decimal fiatTransactionAmount = request.TotalCost - request.BonusAmount;
        decimal fiatCashBack = fiatTransactionAmount * ((decimal) request.FiatCashbackRate / 100m);
        return (fiatTransactionAmount, fiatCashBack);
    }
    private async Task<TransactionDto> CreateBonusTransactionAsync(StoreDto store, TransactionRequestDto request)
    {
        decimal commissionPercent = request.CommissionPercent ?? _commissionBffService.GetDefaultCommissionPercent();
        var transaction = new TransactionDto
        {
            Id = Guid.NewGuid(),
            UserId = request.BuyerId,
            CompanyId = store.CompanyId,
            StoreId = store.Id,
            SellerId = request.SellerId,
            BonusAmount = request.BonusAmount,
            TotalCost = request.TotalCost,
            Type = request.Type,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Pending,
            Description = $"Transaction at {store.Name}",
            CommissionPercent = commissionPercent
        };

        await _dataService.Transactions.CreateAsync(transaction);

        return transaction;
    }
    private async Task<FiatTransactionDto> CreateFiatTransactionAsync(Guid sellerId, StoreDto store, FiatTransactionRequestDto request)
    {
        var (fiatTransactionAmount, fiatCashBack) = CalculateFiatValues(request);
        decimal commissionPercent = request.CommissionPercent ?? _commissionBffService.GetDefaultCommissionPercent();

        //Create the fiat transaction
        var fiatTransaction = new FiatTransactionDto
        {
            Id = Guid.NewGuid(),
            UserId = request.BuyerId,
            CompanyId = store.CompanyId,
            StoreId = store.Id,
            BonusAmount = request.BonusAmount,
            TotalCost = request.TotalCost,
            Timestamp = DateTime.UtcNow,
            Status = FiatTransactionStatus.Pending,
            Description = $"Transaction at {store.Name}",
            FiatCashBackRate = request.FiatCashbackRate.Value,
            FiatTransactionAmount = fiatTransactionAmount,
            FiatCashBackAmount = fiatCashBack,
            SellerId = sellerId,
            CommissionPercent = commissionPercent,
            RelatedBonusTransactionId = request.RelatedBonusTransactionId,
            
        };

        await _dataService.FiatTransactions.CreateAsync(fiatTransaction);

        return fiatTransaction;
    }

    private async Task UpdateTransactionBalances(UserDto buyer, CompanyDto? company, TransactionRequestDto request)
    {
        decimal newBuyerBonusBalance = buyer.BonusBalance;
        decimal newCompanyBonusBalance = company.BonusBalance;

        switch (request.Type)
        {
            case TransactionType.Earn:
                newBuyerBonusBalance += request.BonusAmount;
                newCompanyBonusBalance -= request.BonusAmount;
                break;
            case TransactionType.Spend:
                newCompanyBonusBalance += request.BonusAmount;
                newBuyerBonusBalance -= request.BonusAmount;
                break;
        }
        await _dataService.Users.UpdateBalanceAsync(buyer.Id, newBuyerBonusBalance, buyer.BonusBalance);
        await _dataService.Companies.UpdateBalanceAsync(company.Id, newCompanyBonusBalance, company.BonusBalance);
    
    }
    private async Task UpdateFiatTransactionBalances(UserDto buyer, CompanyDto? company, FiatTransactionRequestDto request)
    {
        var (fiatTransactionAmount, fiatCashBack) = CalculateFiatValues(request);

        decimal newBuyerFiatBalance = buyer.FiatBalance - fiatTransactionAmount;
        decimal newCompanyFiatBalance = company.FiatBalance + fiatTransactionAmount;


        await _dataService.Users.UpdateFiatBalanceAsync(buyer.Id, newBuyerFiatBalance, buyer.FiatBalance);
        await _dataService.Companies.UpdateFiatBalanceAsync(company.Id, newCompanyFiatBalance, company.FiatBalance);
        
    }
    private async Task UpdateCashBackBalances(UserDto buyer, CompanyDto? company, FiatTransactionDto fiatTransaction)
    {
        if (fiatTransaction.FiatCashBackAmount > 0)
        {
            decimal newBuyerFiatBalance = buyer.FiatBalance + fiatTransaction.FiatCashBackAmount;
            await _dataService.Users.UpdateFiatBalanceAsync(buyer.Id, newBuyerFiatBalance, buyer.FiatBalance);
            if (company != null)
            {
                decimal newCompanyFiatBalance = company.FiatBalance - fiatTransaction.FiatCashBackAmount;
                await _dataService.Companies.UpdateFiatBalanceAsync(company.Id, newCompanyFiatBalance, company.FiatBalance);
            }
            await _dataService.FiatTransactions.UpdateFiatTransactionStatusAsync(fiatTransaction.Id, FiatTransactionStatus.Completed);
        }
    }

    /// <summary>
    /// Create pending fiat transaction
    /// </summary>
    public async Task<FiatTransactionResultDto> CreatePendingFiatTransactionAsync(Guid sellerId, FiatTransactionRequestDto request)
    {
        FiatTransactionDto? fiatTransactionDto = null;
        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var (fiatTransactionAmount, fiatCashBack) = CalculateFiatValues(request);
            var (sellerDto, buyerDto, storeDto, companyDto) = await ValidateTransactionParticipants(sellerId, request.BuyerId);

            decimal commissionPercent = request.CommissionPercent ?? _commissionBffService.GetDefaultCommissionPercent();

            fiatTransactionDto = new FiatTransactionDto
            {
                Id = Guid.NewGuid(),
                SellerId = sellerId,
                UserId = buyerDto.Id,
                StoreId = storeDto.Id,
                CompanyId = companyDto?.Id,
                FiatTransactionAmount = fiatTransactionAmount,
                FiatCashBackRate = (decimal)(request.FiatCashbackRate ?? 0),
                FiatCashBackAmount = fiatCashBack,
                Status = FiatTransactionStatus.Pending,
                Timestamp = DateTime.UtcNow,
                CommissionPercent = commissionPercent
            };
            await _dataService.FiatTransactions.CreatePendingTransactionAsync(fiatTransactionDto);

            await _dataService.Notifications.SendNotificationAsync(buyerDto.Id, Res.Format("Notification.ConfirmFiatTransactionAmount", fiatTransactionAmount), NotificationType.Transaction);
        });
        return new FiatTransactionResultDto
        {
            Success = true,
            FiatTransaction = fiatTransactionDto,
        };
    }
}

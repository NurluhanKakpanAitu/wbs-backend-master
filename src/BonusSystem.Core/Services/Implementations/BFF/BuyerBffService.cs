using BonusSystem.Application.Common.Transactions;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.Extensions.Logging;
using BonusSystem.Localization;
using Microsoft.Extensions.Configuration;

namespace BonusSystem.Core.Services.Implementations.BFF;

/// <summary>
/// BFF service for Buyer role
/// </summary>
public class BuyerBffService : BaseBffService, IBuyerBffService
{
    private readonly ILogger<BuyerBffService> _logger;
    private readonly ITransactionExecutor _executor;
    private readonly IConfiguration _configuration;

    public BuyerBffService(
        IDataService dataService,
        IAuthenticationService authService,
        ILogger<BuyerBffService> logger,
        ITransactionExecutor executor,
        IConfiguration configuration)
        : base(dataService, authService)
    {
        _logger = logger;
        _executor = executor;
        _configuration = configuration; 
    }
    public async Task<UserDto> GetUserContextAsync(Guid userId)
    {
        var user = await _dataService.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException(Res.Format("Error.UserNotFoundWithId", userId));
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
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
    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId)
    {
        var notifications = await _dataService.Notifications.GetUserNotificationsAsync(userId);
        return notifications;
    }
    public async Task<DateTime?> GetDateBonusRemove(Guid userId)
    {
        var user = await _dataService.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException(Res.Format("Error.UserNotFoundWithId", userId));
        }

        var currentDate = DateTime.UtcNow;
        var quarter = (currentDate.Month - 1) / 3 + 1;
        var nextQuarterEndDate = new DateTime(currentDate.Year, quarter * 3, 1).AddMonths(1).AddDays(-1); // calculate the next quarter's end date

        return nextQuarterEndDate;
    }
    /// <summary>
    /// Gets the permitted actions for a buyer
    /// </summary>  
    public override async Task<IEnumerable<PermittedActionDto>> GetPermittedActionsAsync(Guid userId)
    {
        var role = await _dataService.Users.GetUserRoleAsync(userId);
        if (role != UserRole.Buyer)
        {
            return Enumerable.Empty<PermittedActionDto>();
        }

        return new List<PermittedActionDto>
        {
            new() { ActionName = "ViewBalance", Description = "View bonus balance", Endpoint = "/api/buyers/balance" },
            new() { ActionName = "ViewTransactions", Description = "View transaction history", Endpoint = "/api/buyers/transactions" },
            new() { ActionName = "ViewFiatTransactions", Description = "View fiat transaction history", Endpoint = "/api/buyers/fiatTransactions" },
            new() { ActionName = "GenerateQrCode", Description = "Generate QR code", Endpoint = "/api/buyers/qrcode" },
            new() { ActionName = "FindStores", Description = "Find stores by category", Endpoint = "/api/buyers/stores" },
            new() { ActionName = "ApproveBonusTransactionReturn", Description = "Approve bonus transaction return", Endpoint = "/api/buyers/transactionReturns/{id}/approveBonus"},
            new() { ActionName = "ApproveFiatTransactionReturn", Description = "Approve fiat transaction return", Endpoint = "/api/buyers/transactionReturns/{id}/approveFiat"},
            new () { ActionName = "GetApprovedReturnsByUser", Description= "Get user approved transaction returns", Endpoint = "/api/buyers/transactionReturns/approvedList" },
            new () { ActionName = "GetPendingReturnsToApprove", Description="Get user pending for approval transaction returns", Endpoint="/api/buyers/transactionReturns/pendingList"},
            new () { ActionName = "RejectTransactionReturn", Description="Reject transaction return async", Endpoint="/api/buyers/transactionReturns/{id}/rejectReturn"}
        };
    }
    /// <summary>
    /// Transfers bonus points from one user to another.
    /// </summary>
    /// <param name="request">The transfer request containing sender and recipient IDs and the amount to transfer.</param>
    /// <returns>A result object indicating the success of the transfer and details of the transaction.</returns>
    /// <exception cref="ArgumentException">Thrown if sender or recipient is not found.</exception>

    public async Task<TransferResultDto> TransferBonusesAsync(Guid senderId, TransferForm request)
    {
        // Input validation
        if (senderId == Guid.Empty)
            throw new ArgumentException("Sender ID cannot be empty", nameof(senderId));
        
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        
        if (request.RecipientId == Guid.Empty)
            throw new ArgumentException("Recipient ID cannot be empty", nameof(request.RecipientId));
        
        if (request.Amount <= 0)
            throw new ArgumentException("Transfer amount must be positive", nameof(request.Amount));

        // Service availability check
        if (_dataService?.Users == null)
            throw new InvalidOperationException("User data service is not available");

        const int maxRetries = 3;
        TransferDto? transferDto = null;

        // Retry loop to handle concurrency conflicts
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await _executor.ExecuteWithRetryAsync(async () =>
                {
                    // Get fresh data for both users on each attempt
                    var sender = await _dataService.Users.GetByIdAsync(senderId);
                    if (sender == null)
                    {
                        _logger?.LogWarning("Sender with ID {SenderId} not found", senderId);
                        throw new InvalidOperationException(Res.Get("Error.SenderNotFound"));
                    }

                    var recipient = await _dataService.Users.GetByIdAsync(request.RecipientId);
                    if (recipient == null)
                    {
                        _logger?.LogWarning("Recipient with ID {RecipientId} not found", request.RecipientId);
                        throw new BusinessException(Res.Get("Error.RecipientNotFound"));
                    }

                    // Check balance with current values
                    if (sender.BonusBalance < request.Amount)
                    {
                        _logger?.LogWarning("Insufficient balance for user {SenderId}. Current: {Current}, Requested: {Requested}", 
                            senderId, sender.BonusBalance, request.Amount);
                        throw new BusinessException(Res.Get("Error.InsufficientBonusBalanceForTransfer"));
                    }

                    var senderCurrentBalance = sender.BonusBalance;
                    var recipientCurrentBalance = recipient.BonusBalance;
                    var senderNewBalance = senderCurrentBalance - request.Amount;
                    var recipientNewBalance = recipientCurrentBalance + request.Amount;

                    _logger?.LogDebug("Attempt {Attempt}: Updating balances - Sender: {SenderId} ({Current} -> {New}), Recipient: {RecipientId} ({Current} -> {New})", 
                        attempt, senderId, senderCurrentBalance, senderNewBalance, request.RecipientId, recipientCurrentBalance, recipientNewBalance);

                    // Try to update sender balance first
                    var senderUpdateSuccess = await _dataService.Users.UpdateBalanceAsync(
                        sender.Id, senderNewBalance, senderCurrentBalance);

                    if (!senderUpdateSuccess)
                    {
                        _logger?.LogWarning("Attempt {Attempt}: Sender balance update failed for user {SenderId} - concurrent modification detected", 
                            attempt, senderId);
                        throw new InvalidOperationException($"Concurrent modification detected for sender {senderId}");
                    }

                    // Try to update recipient balance
                    var recipientUpdateSuccess = await _dataService.Users.UpdateBalanceAsync(
                        recipient.Id, recipientNewBalance, recipientCurrentBalance);

                    if (!recipientUpdateSuccess)
                    {
                        _logger?.LogWarning("Attempt {Attempt}: Recipient balance update failed for user {RecipientId} - concurrent modification detected", 
                            attempt, request.RecipientId);

                        // Rollback sender balance
                        try
                        {
                            await _dataService.Users.UpdateBalanceAsync(
                                sender.Id, senderCurrentBalance, senderNewBalance);
                            _logger?.LogDebug("Successfully rolled back sender balance for user {SenderId}", senderId);
                        }
                        catch (Exception rollbackEx)
                        {
                            _logger?.LogError(rollbackEx, "CRITICAL: Failed to rollback sender balance for user {SenderId}. Manual intervention required!", senderId);
                            // This is a critical error - sender balance was debited but recipient wasn't credited
                            // and we couldn't rollback. This needs immediate attention.
                            throw new InvalidOperationException($"Critical error: Failed to rollback sender balance after recipient update failure. User {senderId} may have incorrect balance.", rollbackEx);
                        }

                        throw new InvalidOperationException($"Concurrent modification detected for recipient {request.RecipientId}");
                    }

                    // Both balance updates succeeded, create transfer record
                    transferDto = new TransferDto
                    {
                        Id = Guid.NewGuid(),
                        Type = TransactionType.Transfer,
                        SenderId = sender.Id,
                        RecipientId = recipient.Id,
                        Amount = request.Amount,
                        Timestamp = DateTime.UtcNow,
                        Status = TransactionStatus.Completed,
                        CommissionPercent = 5
                    };

                    try
                    {
                        await _dataService.Transfers.CreateAsync(transferDto);
                        _logger?.LogInformation("Successfully transferred {Amount} bonus points from {SenderId} to {RecipientId} on attempt {Attempt}", 
                            request.Amount, senderId, request.RecipientId, attempt);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to create transfer record after successful balance updates. Transfer completed but not recorded properly.");
                        // Balance updates succeeded but transfer record failed - this is less critical but should be logged
                        // The transfer actually happened, just wasn't recorded properly
                    }
                });

                // If we get here, the transfer was successful
                return new TransferResultDto { Success = true, Transfer = transferDto };
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrent modification") && attempt < maxRetries)
            {
                _logger?.LogInformation("Concurrency conflict on attempt {Attempt}/{MaxAttempts} for transfer from {SenderId} to {RecipientId}. Retrying...", 
                    attempt, maxRetries, senderId, request.RecipientId);
                
                // Wait a bit before retrying to reduce contention
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt));
                continue;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Transfer failed on attempt {Attempt}: {Error}", attempt, ex.Message);
                throw;
            }
        }

        // If we exhausted all retries
        var errorMessage = $"Failed to transfer bonus points after {maxRetries} attempts due to concurrent modifications. Please try again.";
        _logger?.LogError("Transfer from {SenderId} to {RecipientId} failed after {MaxRetries} attempts", 
            senderId, request.RecipientId, maxRetries);
        throw new InvalidOperationException(errorMessage);
    }

    public async Task<PagedResult<TransferHistoryDto>> GetTransfersAsync(Guid userId, int page, int pageSize)
    {
        return await _dataService.Users.GetTransfersForUserAsync(userId, page, pageSize);
    }
    public async Task<PagedResult<TransferHistoryDto>> GetReplenishmentsForBuyerAsync(Guid buyerId, int page, int pageSize)
    {
        return await _dataService.Users.GetReplenishmentsAsync(buyerId, page, pageSize); 
    }
    /// <summary>
    /// Gets the bonus summary for a buyer
    /// </summary>
    public async Task<BonusTransactionSummaryDto> GetBonusSummaryAsync(Guid userId)
    {
        var transactions = await _dataService.Transactions.GetTransactionsByUserIdAsync(userId);

        // Calculate earned and spent amounts
        var earned = transactions
            .Where(t => t.Type == TransactionType.Earn && t.Status == TransactionStatus.Completed)
            .Sum(t => t.BonusAmount);

        var spent = transactions
            .Where(t => t.Type == TransactionType.Spend && t.Status == TransactionStatus.Completed)
            .Sum(t => t.BonusAmount);

        // Get current user balance
        var user = await _dataService.Users.GetByIdAsync(userId);
        var currentBalance = user?.BonusBalance ?? 0m;

        // Get expiring amount - for prototype, this is just a placeholder
        // In a real implementation, this would consider the quarterly expiration rules
        var expiringNextQuarter = currentBalance * 0.5m;

        // Get recent transactions
        var recentTransactions = transactions
            .OrderByDescending(t => t.Timestamp)
            .Take(5)
            .ToList();

        return new BonusTransactionSummaryDto
        {
            TotalEarned = earned,
            TotalSpent = spent,
            CurrentBalance = currentBalance,
            ExpiringNextQuarter = expiringNextQuarter,
            RecentTransactions = recentTransactions
        };
    }

    public async Task<TransferResultDto> ReplenishWalletAsync(Guid userId, decimal amount)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Sender ID cannot be empty", nameof(userId));
        
        if (amount <= 0)
            throw new ArgumentException("Replenishment amount must be positive", nameof(amount));

        if (_dataService?.Users == null)
            throw new InvalidOperationException("User data service is not available");

        var systemUserIdString = _configuration["AppDb:SystemAccountId"];

        if (!Guid.TryParse(systemUserIdString, out var systemUserId))
        {
            _logger.LogError("SystemUser:Id is not a valid Guid in configuration.");
            throw new InvalidOperationException("System user ID is not configured correctly in appsettings.json (e.g., 'SystemUser:Id').");
        }

        TransferDto? transferDto = null;
        try
        {
            await _executor.ExecuteWithRetryAsync(async () =>
            {
                var recipient = await _dataService.Users.GetByIdAsync(userId);
                if (recipient == null)
                {
                    _logger?.LogWarning("Recipient with ID {userId} not found", userId);
                    throw new BusinessException(Res.Get("Error.RecipientNotFound"));
                }

                var recipientCurrentBalance = recipient.FiatBalance;
                var recipientNewBalance = recipient.FiatBalance + amount;

                var recipientUpdateSuccess = await _dataService.Users.UpdateFiatBalanceAsync(
                    recipient.Id, recipientNewBalance, recipientCurrentBalance);

                if (!recipientUpdateSuccess)
                {
                    _logger?.LogWarning("Recipient fiat balance update failed for user {RecipientId} - concurrent modification detected", userId);
                    throw new InvalidOperationException($"Concurrent modification detected for recipient {userId}");
                }

                transferDto = new TransferDto
                {
                    Id = Guid.NewGuid(),
                    TransactionId = Guid.NewGuid(),
                    Type = TransactionType.Replenishment,
                    SenderId = systemUserId,
                    RecipientId = recipient.Id,
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    Status = TransactionStatus.Completed,
                    CommissionPercent = 0
                };

                await _dataService.Transfers.CreateAsync(transferDto);
                _logger?.LogInformation("Successfully replenished {Amount} fiat currency for {RecipientId}. System Sender: {SenderId}",
                    amount, recipient.Id, systemUserId);
            });

            return new TransferResultDto { Success = true, Transfer = transferDto };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Fiat wallet replenishment failed for user {UserId} on attempt: {Error}", userId, ex.Message);
            throw;
        }
    }
    public async Task<FiatBalanceDto> GetFiatBalanceAsync(Guid userId)
    {
        var user = await _dataService.Users.GetByIdAsync(userId);
        return new FiatBalanceDto
        {
            FiatBalance = user?.FiatBalance ?? 0m
        };
    }
    /// <summary>
    /// Gets the transaction history for a buyer
    /// </summary>
    public async Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(Guid userId)
    {
        var transactions = await _dataService.Transactions.GetTransactionsByUserIdAsync(userId);
        return transactions.OrderByDescending(t => t.Timestamp);
    }

    /// <summary>
    /// Generates a QR code for a buyer
    /// </summary>
    public Task<string> GenerateQrCodeAsync(Guid userId)
    {
        // In a real implementation, this would generate an actual QR code
        // For the prototype, we'll just return a placeholder
        var qrData = $"BONUS-USER-{userId:N}";
        return Task.FromResult(qrData);
    }

    /// <summary>
    /// Finds stores by category
    /// </summary>
    public async Task<IEnumerable<StoreDto>> FindStoresByCategoryAsync(string category)
    {
        return await _dataService.Stores.GetStoresByCategoryAsync(category);
    }

    public async Task<PagedResult<CompanyStoresDto>> FindStoresAsync(StoreFilterRequestDto filter)
    {
        var companies = await _dataService.Companies.GetCompanySummariesAsync();

        var pagedCompanies = companies
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var resultItems = new List<CompanyStoresDto>();

        foreach (var company in pagedCompanies)
        {
            var stores = (await _dataService.Stores.GetStoresByCompanyIdAsync(company.Id))
                .Where(s => filter.CategoryId == 0 || s.CategoryId == filter.CategoryId)
                .ToList();

            resultItems.Add(new CompanyStoresDto
            {
                Name = company.Name,
                Stores = stores
            });
        }

        return new PagedResult<CompanyStoresDto>
        {
            Items = resultItems,
            TotalCount = companies.Count(),
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(int page, int pageSize)
    {
        var total = await _dataService.Categories.GetCountAsync();
        var categories = await _dataService.Categories.GetPagedAsync(page, pageSize);

        var items = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        return new PagedResult<CategoryDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Lists all pending transaction returns where the buyer is expected to approve
    /// </summary>
    public async Task<IEnumerable<TransactionReturnDto>> GetPendingReturnsToApproveAsync(Guid buyerId)
    {
        var allPendingReturns = await _dataService.TransactionReturns.GetPendingReturnsByBuyerIdAsync(buyerId);

        return allPendingReturns;
    }
    /// <summary>
    /// Gets all transaction returns approved by the specified buyer
    /// </summary>
    public async Task<IEnumerable<TransactionReturnDto>> GetApprovedReturnsByUserAsync(Guid buyerId)
    {
        var approvedReturns = await _dataService.TransactionReturns
            .GetByApprovedUserIdAsync(buyerId);

        return approvedReturns;
    }

    public async Task<bool> ApproveFiatTransactionReturnAsync(Guid buyerId, Guid fiatTransactionId)
    {
        try
        {
            bool success = false;

            await _executor.ExecuteWithRetryAsync(async () =>
            {
                var buyer = await _dataService.Users.GetByIdAsync(buyerId);
                if (buyer == null || buyer.Role != UserRole.Buyer)
                    throw new InvalidOperationException(Res.Get("Error.InvalidBuyer"));

                var fiatTransaction = await _dataService.FiatTransactions.GetByIdAsync(fiatTransactionId);
                if (fiatTransaction == null)
                    throw new InvalidOperationException(Res.Get("Error.FiatTransactionNotFound"));

                var returnRequest = await _dataService.TransactionReturns
                    .GetByFiatTransactionIdAsync(fiatTransactionId, TransactionReturnStatus.Pending);
                if (returnRequest == null)
                    throw new InvalidOperationException(Res.Get("Error.NoPendingReturnRequestFound"));

                var company = await _dataService.Companies.GetByIdAsync(fiatTransaction.CompanyId!.Value);
                if (company == null)
                    throw new InvalidOperationException(Res.Get("Error.CompanyNotFound"));

                if (fiatTransaction.Status != FiatTransactionStatus.Completed ||
                    fiatTransaction.Timestamp < DateTime.UtcNow.AddDays(-7))
                    throw new InvalidOperationException(Res.Get("Error.FiatTransactionCannotBeReversed"));

                await UpdateReverseFiatBalances(buyer, company, fiatTransaction);

                // Update return record
                returnRequest.Status = TransactionReturnStatus.Approved;
                returnRequest.ApprovedByUserId = buyerId;
                returnRequest.ApprovedAt = DateTime.UtcNow;

                await _dataService.TransactionReturns.UpdateAsync(returnRequest);

                success = true;
            });

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving return for fiat transaction {FiatTransactionId}", fiatTransactionId);
            return false;
        }
    }
    public async Task<bool> ApproveBonusTransactionReturnAsync(Guid buyerId, Guid transactionId)
    {
        try
        {
            bool success = false;

            await _executor.ExecuteWithRetryAsync(async () =>
            {
                var buyer = await _dataService.Users.GetByIdAsync(buyerId);
                if (buyer == null || buyer.Role != UserRole.Buyer)
                    throw new InvalidOperationException(Res.Get("Error.InvalidBuyer"));

                var transaction = await _dataService.Transactions.GetByIdAsync(transactionId);
                if (transaction == null)
                    throw new InvalidOperationException(Res.Get("Error.TransactionNotFound"));

                var returnRequest = await _dataService.TransactionReturns
                    .GetByBonusTransactionIdAsync(transactionId, TransactionReturnStatus.Pending);
                if (returnRequest == null)
                    throw new InvalidOperationException(Res.Get("Error.NoPendingReturnRequestFound"));

                if (transaction.Status != TransactionStatus.Completed ||
                    transaction.Timestamp < DateTime.UtcNow.AddDays(-7))
                    throw new InvalidOperationException(Res.Get("Error.TransactionCannotBeReversed"));

                // Update transaction status
                await _dataService.Transactions.UpdateTransactionStatusAsync(transactionId, TransactionStatus.Reversed);

                // Adjust balances
                if (transaction.UserId.HasValue)
                {
                    var user = await _dataService.Users.GetByIdAsync(transaction.UserId.Value);
                    if (user != null)
                    {
                        decimal newBalance = transaction.Type switch
                        {
                            TransactionType.Earn => user.BonusBalance - transaction.BonusAmount,
                            TransactionType.Spend => user.BonusBalance + transaction.BonusAmount,
                            _ => user.BonusBalance
                        };

                        await _dataService.Users.UpdateBalanceAsync(user.Id, newBalance, user.BonusBalance);
                    }
                }

                if (transaction.CompanyId.HasValue)
                {
                    var company = await _dataService.Companies.GetByIdAsync(transaction.CompanyId.Value);
                    if (company != null)
                    {
                        decimal newBalance = transaction.Type switch
                        {
                            TransactionType.Earn => company.BonusBalance + transaction.BonusAmount,
                            TransactionType.Spend => company.BonusBalance - transaction.BonusAmount,
                            _ => company.BonusBalance
                        };

                        await _dataService.Companies.UpdateBalanceAsync(company.Id, newBalance, company.BonusBalance);
                    }
                }

                // Update return record
                returnRequest.Status = TransactionReturnStatus.Approved;
                returnRequest.ApprovedByUserId = buyerId;
                returnRequest.ApprovedAt = DateTime.UtcNow;

                await _dataService.TransactionReturns.UpdateAsync(returnRequest);

                success = true;
            });

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving return for bonus transaction {TransactionId}", transactionId);
            return false;
        }
    }
    public async Task<bool> RejectTransactionReturnAsync(Guid userId, Guid returnId)
    {
        try
        {
            var transactionReturn = await _dataService.TransactionReturns.GetByIdAsync(returnId);
            if (transactionReturn == null || transactionReturn.Status != TransactionReturnStatus.Pending)
            {
                _logger.LogWarning("Cannot reject transaction return {ReturnId} - not found or already processed", returnId);
                return false;
            }

            transactionReturn.Status = TransactionReturnStatus.Rejected;
            transactionReturn.ApprovedByUserId = userId;

            var updated = await _dataService.TransactionReturns.UpdateAsync(transactionReturn);
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting transaction return {ReturnId}", returnId);
            return false;
        }
    }
    /// <summary>
    /// Gets the fiat transaction history for a buyer
    /// </summary>
    public async Task<IEnumerable<FiatTransactionDto>> GetFiatTransactionHistoryAsync(Guid userId)
    {
        var fiatTransactions = await _dataService.FiatTransactions.GetFiatTransactionsByUserIdAsync(userId);
        return fiatTransactions.OrderByDescending(t => t.Timestamp);
    }

    private async Task UpdateReverseFiatBalances(UserDto buyer, CompanyDto? company, FiatTransactionDto fiatTransaction)
    {
        if (fiatTransaction.TotalCost > 0)
        {
            decimal reverseAmount = fiatTransaction.FiatTransactionAmount - fiatTransaction.FiatCashBackAmount;

            decimal newBuyerFiatBalance = buyer.FiatBalance + reverseAmount;
            await _dataService.Users.UpdateFiatBalanceAsync(buyer.Id, newBuyerFiatBalance, buyer.FiatBalance);
            if (company != null)
            {
                decimal newCompanyFiatBalance = company.FiatBalance - reverseAmount;
                await _dataService.Companies.UpdateFiatBalanceAsync(company.Id, newCompanyFiatBalance, company.FiatBalance);
            }
            await _dataService.FiatTransactions.UpdateFiatTransactionStatusAsync(fiatTransaction.Id, FiatTransactionStatus.Reversed);
        }
    }
    public async Task<bool> ConfirmPendingFiatTransactionAsync(Guid buyerId, Guid fiatTransactionId)
    {
        bool success = false;
        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var buyer = await _dataService.Users.GetByIdAsync(buyerId);
            if (buyer == null || buyer.Role != UserRole.Buyer)
                throw new InvalidOperationException(Res.Get("Error.InvalidBuyer"));

            var fiatTransaction = await _dataService.FiatTransactions.GetByIdAsync(fiatTransactionId);
            if (fiatTransaction == null || fiatTransaction.UserId != buyerId)
                throw new InvalidOperationException(Res.Get("Error.TransactionNotFoundOrNotOwnedByBuyer"));

            if (fiatTransaction.Status != FiatTransactionStatus.Pending)
                throw new InvalidOperationException(Res.Get("Error.TransactionNotPending"));

            if (buyer.FiatBalance < fiatTransaction.FiatTransactionAmount)
                throw new BusinessException(Res.Get("Error.InsufficientFiatBalance"));

            decimal newBuyerBalance = buyer.FiatBalance - fiatTransaction.FiatTransactionAmount;
            await _dataService.Users.UpdateFiatBalanceAsync(buyer.Id, newBuyerBalance, buyer.FiatBalance);

            if (fiatTransaction.SellerId.HasValue)
            {
                var seller = await _dataService.Users.GetByIdAsync(fiatTransaction.SellerId.Value);
                if (seller != null)
                {
                    decimal newSellerBalance = seller.FiatBalance + fiatTransaction.FiatTransactionAmount;
                    await _dataService.Users.UpdateFiatBalanceAsync(seller.Id, newSellerBalance, seller.FiatBalance);
                }
            }

            await _dataService.FiatTransactions.UpdateFiatTransactionStatusAsync(fiatTransactionId, FiatTransactionStatus.Confirmed);

            if (fiatTransaction.SellerId.HasValue)
            {
                await _dataService.Notifications.SendNotificationAsync(fiatTransaction.SellerId.Value, Res.Format("Notification.BuyerConfirmedFiatTransactionAmount", fiatTransaction.FiatTransactionAmount), NotificationType.Transaction);
            }

            success = true;
        });
        return success;
    }

    public async Task<bool> ConfirmPendingBonusTransactionAsync(Guid buyerId, Guid transactionId)
    {
        bool success = false;
        await _executor.ExecuteWithRetryAsync(async () =>
        {
            var transaction = await _dataService.Transactions.GetByIdAsync(transactionId);
            if (transaction == null || transaction.UserId != buyerId || transaction.Status != TransactionStatus.Pending)
                throw new InvalidOperationException("Transaction not found or not pending for this user");

            await _dataService.Transactions.UpdateTransactionStatusAsync(transactionId, TransactionStatus.Completed);

            Guid? sellerId = null;
            if (transaction.StoreId.HasValue)
            {
                var store = await _dataService.Stores.GetByIdAsync(transaction.StoreId.Value);
            }
            if (sellerId.HasValue)
            {
                await _dataService.Notifications.SendNotificationAsync(sellerId.Value, Res.Format("Notification.BuyerConfirmedBonusTransactionAmount", transaction.BonusAmount), NotificationType.Transaction);
            }
            success = true;
        });
        return success;
    }
    public async Task<PagedResult<CombinedTransactionDto>> AllTransactionsForUser(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _dataService.Users.AllTransactionsForUserAsync(userId, page, pageSize);
    }
}

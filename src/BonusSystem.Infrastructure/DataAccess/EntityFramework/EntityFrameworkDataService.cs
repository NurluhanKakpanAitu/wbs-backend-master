using System.Data;
using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Infrastructure.DataAccess.EntityFramework.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BonusSystem.Shared.Models;
using Microsoft.Extensions.Options;
using BonusSystem.Shared.Dtos;
using BonusSystem.Core.Common.IDGenerator; 

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework;

public class EntityFrameworkDataService : IDataService
{
    private readonly BonusSystemContext _context;
    private readonly ILoggerFactory _loggerFactory;
    private readonly FiatTransactionOptions _fiatOptions;

    public EntityFrameworkDataService(BonusSystemContext context, ILoggerFactory loggerFactory, IOptions<FiatTransactionOptions> fiatOptions)
    {
        _context = context;
        _loggerFactory = loggerFactory;
        _fiatOptions = fiatOptions.Value;
        
        var idGenerator = new IDGenerator();
        Users = new EntityFrameworkUserRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkUserRepository>());
        Companies = new EntityFrameworkCompanyRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkCompanyRepository>());
        Categories = new EntityFrameworkCategoryRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkCategoryRepository>());
        Malls = new EntityFrameworkMallRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkMallRepository>(), idGenerator);
        Stores = new EntityFrameworkStoreRepository(_context, idGenerator, _loggerFactory.CreateLogger<EntityFrameworkStoreRepository>());
        FiatReplenishmentCompanyBalances = new EntityFrameworkFiatReplenishmentCompanyBalanceRepository(_context); 
        Transactions = new EntityFrameworkTransactionRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkTransactionRepository>());
        FiatTransactions = new EntityFrameworkFiatTransactionRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkFiatTransactionRepository>(), fiatOptions, idGenerator);
        Notifications = new EntityFrameworkNotificationRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkNotificationRepository>());
        Transfers = new EntityFrameworkTransferRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkTransferRepository>());
        TransactionReturns = new EntityFrameworkTransactionReturnRepository(_context, _loggerFactory.CreateLogger<EntityFrameworkTransactionReturnRepository>());
    }
    public IMallRepository Malls { get; }
    public IUserRepository Users { get; }
    public ICompanyRepository Companies { get; }
    public IStoreRepository Stores { get; }
    public ITransactionRepository Transactions { get; }
    public ITransferRepository Transfers { get; }
    public ITransactionReturnRepository TransactionReturns { get; } 
    public IFiatReplenishmentCompanyBalanceRepository FiatReplenishmentCompanyBalances { get; }

    public IFiatTransactionRepository FiatTransactions { get; }
    public INotificationRepository Notifications { get; }
    public ICategoryRepository Categories { get; }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.Serializable)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(isolationLevel);

            try
            {
                await operation();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<IReadOnlyList<TransactionDto>> GetPendingBonusTransactionsAsync()
    {
        // Получить все бонусные транзакции со статусом Pending
        return (await _context.BonusTransactions
            .Where(t => t.Status == TransactionStatus.Pending)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                CompanyId = t.CompanyId,
                StoreId = t.StoreId,
                BonusAmount = t.BonusAmount,
                TotalCost = t.TotalCost,
                Type = t.Type,
                Timestamp = t.Timestamp,
                Status = t.Status,
                Description = t.Description ?? string.Empty
            })
            .ToListAsync());
    }

    public async Task<IReadOnlyList<FiatTransactionDto>> GetPendingFiatTransactionsAsync()
    {
        // Получить все фиатные транзакции со статусом Pending
        return (await _context.FiatTransactions
            .Where(t => t.Status == FiatTransactionStatus.Pending)
            .Select(t => new FiatTransactionDto
            {
                Id = t.Id,
                FrontendId = t.FrontendId,
                UserId = t.UserId,
                CompanyId = t.CompanyId,
                StoreId = t.StoreId,
                SellerId = t.SellerId,
                BonusAmount = t.BonusAmount,
                TotalCost = t.TotalCost,
                FiatCashBackRate = t.FiatCashBackRate,
                FiatTransactionAmount = t.FiatTransactionAmount,
                FiatCashBackAmount = t.FiatCashBackAmount,
                Timestamp = t.Timestamp,
                Status = t.Status,
                Description = t.Description ?? string.Empty,
                CommissionPercent = t.CommissionPercent
            })
            .ToListAsync());
    }
}
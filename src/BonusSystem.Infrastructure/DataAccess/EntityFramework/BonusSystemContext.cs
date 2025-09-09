using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework;

public sealed class BonusSystemContext : DbContext
{
    public BonusSystemContext(DbContextOptions<BonusSystemContext> options) : base(options)
    {
        // Do not call Database.EnsureCreated() here as it conflicts with migrations
    }
    
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<CompanyEntity> Companies { get; set; } = null!;
    public DbSet<StoreEntity> Stores { get; set; } = null!;
    public DbSet<CategoryEntity> Categories { get; set; } = null!;
    public DbSet<MallEntity> Malls { get; set; } = null!; 
    public DbSet<BonusTransactionEntity> BonusTransactions { get; set; } = null!;
    public DbSet<FiatTransactionEntity> FiatTransactions { get; set; } = null!;
    public DbSet<TransferEntity> Transfers { get; set; } = null!;
    public DbSet<TransactionReturn> TransactionReturns { get; set; } = null!;
    public DbSet<NotificationEntity> Notifications { get; set; } = null!; 
    public DbSet<FiatReplenishmentCompanyBalanceEntity> FiatReplenishmentCompanyBalances { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema name
        modelBuilder.HasDefaultSchema("bonus");

        // Apply configurations
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyEntityConfiguration());
        modelBuilder.ApplyConfiguration(new StoreEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BonusTransactionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new FiatTransactionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionReturnEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TransferEntityConfiguration());
        modelBuilder.ApplyConfiguration(new MallEntityConfiguration());
        modelBuilder.ApplyConfiguration(new FiatReplenishmentCompanyBalanceEntityConfiguration()); 
    }
}

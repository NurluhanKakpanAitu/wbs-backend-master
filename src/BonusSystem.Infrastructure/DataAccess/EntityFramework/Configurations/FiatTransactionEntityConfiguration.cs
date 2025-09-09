using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class FiatTransactionEntityConfiguration : IEntityTypeConfiguration<FiatTransactionEntity>
{
    public void Configure(EntityTypeBuilder<FiatTransactionEntity> builder)
    {
        builder.ToTable("fiatTransactions");

        builder.Property(t => t.BonusAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.FiatCashBackRate)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.FiatTransactionAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.FiatCashBackAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.Timestamp)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(t => t.CommissionPercent)
            .HasPrecision(5, 2)
            .IsRequired();

        // Relationships
        builder.HasOne(t => t.User)
            .WithMany(u => u.FiatTransactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Company)
            .WithMany(c => c.FiatTransactions)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Store)
            .WithMany(s => s.FiatTransactions)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.HasOne(t => t.RelatedBonusTransaction)
            .WithMany()
            .HasForeignKey(t => t.RelatedBonusTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
        

        // Indexes
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.CompanyId);
        builder.HasIndex(t => t.StoreId);
        builder.HasIndex(t => t.RelatedBonusTransactionId);
        builder.HasIndex(t => t.Timestamp);
        builder.HasIndex(t => t.Status);
    }
} 

public class FiatReplenishmentCompanyBalanceEntityConfiguration : IEntityTypeConfiguration<FiatReplenishmentCompanyBalanceEntity>
{
    public void Configure(EntityTypeBuilder<FiatReplenishmentCompanyBalanceEntity> builder)
    {
        builder.ToTable("fiatReplenishmentCompanyBalances");

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Timestamp)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        // Relationships
        builder.HasOne(t => t.Company)
            .WithMany(c => c.FiatReplenishmentCompanyBalances)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
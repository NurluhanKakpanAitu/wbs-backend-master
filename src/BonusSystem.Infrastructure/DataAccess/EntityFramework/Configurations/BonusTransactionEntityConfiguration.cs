using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class BonusTransactionEntityConfiguration : IEntityTypeConfiguration<BonusTransactionEntity>
{
    public void Configure(EntityTypeBuilder<BonusTransactionEntity> builder)
    {
        builder.ToTable("transactions");

        builder.Property(t => t.BonusAmount)
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
        
        // Relationships
        builder.HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Company)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Store)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.CompanyId);
        builder.HasIndex(t => t.StoreId);
        builder.HasIndex(t => t.Timestamp);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.Status);
    }
}
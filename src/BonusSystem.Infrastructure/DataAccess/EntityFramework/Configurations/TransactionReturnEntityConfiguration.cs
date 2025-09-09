using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class TransactionReturnEntityConfiguration : IEntityTypeConfiguration<TransactionReturn>
{
    public void Configure(EntityTypeBuilder<TransactionReturn> builder)
    {

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Reason)
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property(t => t.RequestedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Relationships
        builder.HasOne(t => t.BonusTransaction)
            .WithOne()
            .HasForeignKey<TransactionReturn>(t => t.BonusTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.FiatTransaction)
            .WithOne()
            .HasForeignKey<TransactionReturn>(t => t.FiatTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(t => t.RequestedByUser)
            .WithMany()
            .HasForeignKey(t => t.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(t => t.ApprovedByUser)
            .WithMany()
            .HasForeignKey(t => t.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.ToTable("transactionReturns", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_TransactionReturn_OneTransactionType",
                @"(""BonusTransactionId"" IS NOT NULL AND ""FiatTransactionId"" IS NULL) 
                OR (""BonusTransactionId"" IS NULL AND ""FiatTransactionId"" IS NOT NULL)");
        });


        // Indexes
        builder.HasIndex(t => t.Status);
    }
}
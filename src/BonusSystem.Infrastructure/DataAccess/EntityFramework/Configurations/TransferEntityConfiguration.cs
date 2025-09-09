using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BonusSystem.Infrastructure.DataAccess.Entities;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class TransferEntityConfiguration : IEntityTypeConfiguration<TransferEntity>
{
    public void Configure(EntityTypeBuilder<TransferEntity> builder)
    {
        builder.ToTable("Transfers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Sender)
            .WithMany()
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Recipient)
            .WithMany()
            .HasForeignKey(x => x.RecipientId)
            .OnDelete(DeleteBehavior.SetNull);
        // Indexes
        builder.HasIndex(x => x.SenderId);
        builder.HasIndex(x => x.RecipientId);
    }
}

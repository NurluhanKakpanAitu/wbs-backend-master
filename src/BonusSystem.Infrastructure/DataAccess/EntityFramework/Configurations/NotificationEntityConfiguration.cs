using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Message)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Type)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        // Relationships
        builder.HasOne(n => n.Recipient)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.IsRead);
    }
}
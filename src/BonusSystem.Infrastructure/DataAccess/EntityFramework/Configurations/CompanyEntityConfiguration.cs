using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class CompanyEntityConfiguration : IEntityTypeConfiguration<CompanyEntity>
{
    public void Configure(EntityTypeBuilder<CompanyEntity> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FrontendId)
            .HasMaxLength(30)
            .IsRequired(); 
            
        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(c => c.City)
            .HasMaxLength(12)
            .IsRequired(); 
        builder.Property(c => c.Region)
            .HasMaxLength(12)
            .IsRequired(); 
        builder.Property(c => c.ContactEmail)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(c => c.PasswordHash)
            .IsRequired(); 
        builder.Property(c => c.UserName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(c => c.ContactPhone)
            .HasMaxLength(50);

        builder.Property(c => c.BonusBalance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(c => c.OriginalBonusBalance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        // Navigation properties
        builder.HasMany(c => c.Stores)
            .WithOne(s => s.Company)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Company)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Status);
    }
}
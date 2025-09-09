using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");
        
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FrontendId)
            .HasMaxLength(30)
            .IsRequired(); 

        builder.Property(u => u.Username)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(u => u.City)
            .HasMaxLength(100)
            .IsRequired(); 
        builder.Property(u => u.Region)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired(); 
        builder.Property(u => u.INN)
            .HasMaxLength(12)
            .IsRequired();
        builder.Property(u => u.Phone)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(u => u.Phone)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(c => c.PasswordHash)
            .IsRequired(); 
        builder.Property(u => u.Role)
            .IsRequired();
        
        builder.Property(u => u.BonusBalance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();
        
        builder.Property(u => u.IsActive)
            .HasDefaultValue(true); 
        builder.Property(u => u.IsEmailVerified)
            .HasDefaultValue(false); 
        builder.Property(u => u.PincodeSet)
            .HasDefaultValue(false);
        
        // Company relationship
        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Store relationship
        builder.HasOne(u => u.Store)
            .WithMany(c => c.Sellers)
            .HasForeignKey(u => u.StoreId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        // Indexes
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Role);
        builder.HasIndex(u => u.CompanyId);
    }
}

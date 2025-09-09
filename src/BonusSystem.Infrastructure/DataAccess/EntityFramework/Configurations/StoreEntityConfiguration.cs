using BonusSystem.Infrastructure.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class StoreEntityConfiguration : IEntityTypeConfiguration<StoreEntity>
{
    public void Configure(EntityTypeBuilder<StoreEntity> builder)
    {
        builder.ToTable("stores");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.FrontendId)
            .HasMaxLength(30)
            .IsRequired(); 
        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(c => c.PasswordHash)
            .IsRequired();
        builder.Property(c => c.UserName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(s => s.MallId)
            .HasMaxLength(100)
            .IsRequired(); 
        builder.Property(s => s.City)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(s => s.Region)
            .HasMaxLength(100)
            .IsRequired();  
        builder.Property(s => s.Floor)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(s => s.Row)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(s => s.Number)
            .HasMaxLength(50)
            .IsRequired();
        

        builder.Property(s => s.Address)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.ContactPhone)
            .HasMaxLength(50);

        builder.Property(s => s.CategoryId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Status) 
            .IsRequired();

        builder.Property(s => s.BusinessTypeId)
            .IsRequired();

        builder.Property(s => s.Email)
            .IsRequired();

        builder.Property(s => s.WorkingHours)
            .IsRequired();

        builder.Property(s => s.TypeOfbusiness)
            .IsRequired();


        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Relationships
        builder.HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.CompanyId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CategoryId);
        
    }
}
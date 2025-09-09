using BonusSystem.Infrastructure.DataAccess.Entities;
using BonusSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BonusSystem.Infrastructure.DataAccess.EntityFramework.Configurations;

public class MallEntityConfiguration : IEntityTypeConfiguration<MallEntity>
{
    public void Configure(EntityTypeBuilder<MallEntity> builder)
    {
        builder.ToTable("malls");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();
        builder.Property(m => m.FrontendId).IsRequired();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(255);
        builder.Property(m => m.City).IsRequired();
        builder.Property(m => m.Region).IsRequired();
        builder.Property(m => m.WorkHours).IsRequired(); 
    }
}

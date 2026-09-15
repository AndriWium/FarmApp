using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class InputItemConfiguration : IEntityTypeConfiguration<InputItem>
{
    public void Configure(EntityTypeBuilder<InputItem> b)
    {
        b.HasKey(x => x.InputItemId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();

        // First enum in the codebase — stored as string for report readability (doc 11).
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.Property(x => x.Unit).HasMaxLength(20).IsRequired();
        b.Property(x => x.ReorderLevel).HasPrecision(18, 3);
        b.Property(x => x.ActiveIngredient).HasMaxLength(100);
        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

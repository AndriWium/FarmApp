using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.HasKey(x => x.ProductId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        // A product's own name is unique across the whole catalog (matches Grade).
        b.HasIndex(x => x.Name).IsUnique();

        b.Property(x => x.ProductType).HasConversion<string>().HasMaxLength(20).IsRequired();
        // MakeMode only meaningful when ProductType == Prepared, but not hard-enforced (doc brief) - nullable.
        b.Property(x => x.MakeMode).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.BaseUnit).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

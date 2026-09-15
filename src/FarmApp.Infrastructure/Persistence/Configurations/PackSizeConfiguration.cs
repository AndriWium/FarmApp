using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class PackSizeConfiguration : IEntityTypeConfiguration<PackSize>
{
    public void Configure(EntityTypeBuilder<PackSize> b)
    {
        b.HasKey(x => x.PackSizeId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        // Scoped uniqueness: two different products may each have a pack size of the same name.
        b.HasIndex(x => new { x.ProductId, x.Name }).IsUnique();
        b.Property(x => x.QtyInBaseUnit).HasPrecision(18, 3);
    }
}

using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.HasKey(x => x.SupplierId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        // No uniqueness constraint on Name - supplier names are not expected to be unique
        // (two people can coincidentally share a name).
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.VatNumber).HasMaxLength(20);
    }
}

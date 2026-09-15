using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.HasKey(x => x.CustomerId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        // No uniqueness constraint on Name - customer names are not expected to be unique
        // (matches Supplier precedent).
        b.Property(x => x.Phone).HasMaxLength(50);

        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.Property(x => x.CreditLimit).HasPrecision(18, 2);

        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

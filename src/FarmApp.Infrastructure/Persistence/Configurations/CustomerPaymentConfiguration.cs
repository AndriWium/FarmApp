using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
{
    public void Configure(EntityTypeBuilder<CustomerPayment> b)
    {
        b.HasKey(x => x.CustomerPaymentId);

        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Method).HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.Ref).HasMaxLength(50);

        // Supports "payments for this customer" (list + balance SUM), same shape as
        // SalePaymentConfiguration's index on SaleId.
        b.HasIndex(x => x.CustomerId);
    }
}

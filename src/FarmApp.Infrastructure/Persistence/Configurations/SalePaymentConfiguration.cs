using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class SalePaymentConfiguration : IEntityTypeConfiguration<SalePayment>
{
    public void Configure(EntityTypeBuilder<SalePayment> b)
    {
        b.HasKey(x => x.SalePaymentId);

        b.Property(x => x.Method).HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);

        b.HasIndex(x => x.SaleId);
    }
}

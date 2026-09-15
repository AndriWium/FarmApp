using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class ProducePurchaseConfiguration : IEntityTypeConfiguration<ProducePurchase>
{
    public void Configure(EntityTypeBuilder<ProducePurchase> b)
    {
        b.HasKey(x => x.ProducePurchaseId);
        b.Property(x => x.InvoiceRef).HasMaxLength(50);
        b.HasIndex(x => x.SupplierId);
    }
}

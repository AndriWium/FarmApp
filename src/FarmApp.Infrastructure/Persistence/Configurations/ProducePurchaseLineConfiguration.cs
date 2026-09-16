using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class ProducePurchaseLineConfiguration : IEntityTypeConfiguration<ProducePurchaseLine>
{
    public void Configure(EntityTypeBuilder<ProducePurchaseLine> b)
    {
        b.HasKey(x => x.ProducePurchaseLineId);
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
        b.Property(x => x.VatAmount).HasPrecision(18, 2);

        // Supports GetByPurchaseIdAsync's "all lines for this purchase" query.
        b.HasIndex(x => x.ProducePurchaseId);
    }
}

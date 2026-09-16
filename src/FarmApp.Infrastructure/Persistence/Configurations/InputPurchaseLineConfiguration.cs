using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class InputPurchaseLineConfiguration : IEntityTypeConfiguration<InputPurchaseLine>
{
    public void Configure(EntityTypeBuilder<InputPurchaseLine> b)
    {
        b.HasKey(x => x.InputPurchaseLineId);
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
        b.Property(x => x.VatAmount).HasPrecision(18, 2);

        // Supports GetByPurchaseIdAsync's "all lines for this purchase" query.
        b.HasIndex(x => x.InputPurchaseId);
        b.HasIndex(x => x.InputItemId);
    }
}

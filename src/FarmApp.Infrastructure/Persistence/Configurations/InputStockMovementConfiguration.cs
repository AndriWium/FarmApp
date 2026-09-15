using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class InputStockMovementConfiguration : IEntityTypeConfiguration<InputStockMovement>
{
    public void Configure(EntityTypeBuilder<InputStockMovement> b)
    {
        b.HasKey(x => x.InputStockMovementId);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);
        b.Property(x => x.RefTable).HasMaxLength(50);
        b.Property(x => x.Reason).HasMaxLength(200);

        // Supports GetOnHandAsync/GetWeightedAverageCostAsync's "every movement for this item" query.
        b.HasIndex(x => x.InputItemId);
    }
}

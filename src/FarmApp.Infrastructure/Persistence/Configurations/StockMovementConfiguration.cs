using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> b)
    {
        b.HasKey(x => x.StockMovementId);

        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.RefTable).HasMaxLength(50);
        b.Property(x => x.Reason).HasMaxLength(200);

        // Supports GetOnHandAsync's SUM(Qty) WHERE StockBatchId = @id.
        b.HasIndex(x => x.StockBatchId);
    }
}

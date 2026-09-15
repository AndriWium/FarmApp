using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class StockTakeLineConfiguration : IEntityTypeConfiguration<StockTakeLine>
{
    public void Configure(EntityTypeBuilder<StockTakeLine> b)
    {
        b.HasKey(x => x.StockTakeLineId);
        b.Property(x => x.CountedQty).HasPrecision(18, 3);
        b.Property(x => x.SystemQty).HasPrecision(18, 3);
        b.Property(x => x.Variance).HasPrecision(18, 3);

        // Supports GetByStockTakeIdAsync's "all lines for this stock take" query.
        b.HasIndex(x => x.StockTakeId);
        b.HasIndex(x => x.StockBatchId);
    }
}

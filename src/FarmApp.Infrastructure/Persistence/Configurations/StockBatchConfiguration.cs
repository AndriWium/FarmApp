using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> b)
    {
        b.HasKey(x => x.StockBatchId);

        b.Property(x => x.Source).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.Property(x => x.QtyIn).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);

        // Computed from Date/ShelfLifeDays, never persisted (doc 02).
        b.Ignore(x => x.BestBeforeDate);

        // Supports GetAvailableBatchesAsync's "batches for this product/grade, oldest first" query.
        b.HasIndex(x => new { x.ProductId, x.GradeId, x.Date });
    }
}

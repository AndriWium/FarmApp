using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> b)
    {
        b.HasKey(x => x.PriceId);

        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.ValidFrom).HasColumnType("date");
        b.Property(x => x.ValidTo).HasColumnType("date");

        // Backstop for SetPriceAsync's read-then-close-then-insert logic: at most one row can
        // be "current" (ValidTo IS NULL) per (PriceListId, ProductId, GradeId, PackSizeId).
        // See DECISIONS.md.
        b.HasIndex(x => new { x.PriceListId, x.ProductId, x.GradeId, x.PackSizeId })
            .IsUnique()
            .HasFilter("[ValidTo] IS NULL")
            .HasDatabaseName("IX_Prices_ActiveCombo");
    }
}

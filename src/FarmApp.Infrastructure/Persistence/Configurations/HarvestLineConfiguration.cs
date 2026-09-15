using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class HarvestLineConfiguration : IEntityTypeConfiguration<HarvestLine>
{
    public void Configure(EntityTypeBuilder<HarvestLine> b)
    {
        b.HasKey(x => x.HarvestLineId);
        b.Property(x => x.QtyKg).HasPrecision(18, 3);

        // Supports GetByHarvestIdAsync's "all lines for this harvest" query.
        b.HasIndex(x => x.HarvestId);
    }
}

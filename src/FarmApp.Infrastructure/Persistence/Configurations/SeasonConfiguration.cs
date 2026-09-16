using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> b)
    {
        b.HasKey(x => x.SeasonId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // doc 09 costing-estimate fields - all nullable, since a season legitimately starts with
        // no estimate typed in yet.
        b.Property(x => x.ExpectedTotalCost).HasPrecision(18, 2);
        b.Property(x => x.ExpectedYieldKg).HasPrecision(18, 3);
        b.Property(x => x.EstimatedCostPerKg).HasPrecision(18, 2);

        b.HasIndex(x => x.PlantingId);
    }
}

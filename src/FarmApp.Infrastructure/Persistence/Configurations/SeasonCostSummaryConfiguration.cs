using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class SeasonCostSummaryConfiguration : IEntityTypeConfiguration<SeasonCostSummary>
{
    public void Configure(EntityTypeBuilder<SeasonCostSummary> b)
    {
        b.HasKey(x => x.SeasonCostSummaryId);
        b.Property(x => x.InputCost).HasPrecision(18, 2);
        b.Property(x => x.LabourCost).HasPrecision(18, 2);
        b.Property(x => x.OverheadAllocated).HasPrecision(18, 2);
        b.Property(x => x.TotalKgHarvested).HasPrecision(18, 3);
        b.Property(x => x.CostPerKg).HasPrecision(18, 2);
        b.Property(x => x.TrueUpAmount).HasPrecision(18, 2);

        // One summary per season - ConfirmCloseAsync's idempotency guard also checks
        // Season.Status, but the unique index backstops it at the data layer too.
        b.HasIndex(x => x.SeasonId).IsUnique();
    }
}

using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class ActivityInputConfiguration : IEntityTypeConfiguration<ActivityInput>
{
    public void Configure(EntityTypeBuilder<ActivityInput> b)
    {
        b.HasKey(x => x.ActivityInputId);
        b.Property(x => x.Qty).HasPrecision(18, 3);
        b.Property(x => x.UnitCost).HasPrecision(18, 2);

        // Supports GetByActivityIdAsync's "all input lines for this activity" query.
        b.HasIndex(x => x.ActivityId);
        b.HasIndex(x => x.InputItemId);
    }
}

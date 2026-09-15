using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> b)
    {
        b.HasKey(x => x.ActivityId);
        b.Property(x => x.LabourHours).HasPrecision(18, 3);
        b.Property(x => x.LabourCost).HasPrecision(18, 2);
        b.Property(x => x.Notes).HasMaxLength(500);

        b.HasIndex(x => x.SeasonId);
        b.HasIndex(x => x.ActivityTypeId);
    }
}

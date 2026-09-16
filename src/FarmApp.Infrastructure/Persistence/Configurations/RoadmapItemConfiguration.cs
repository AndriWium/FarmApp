using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class RoadmapItemConfiguration : IEntityTypeConfiguration<RoadmapItem>
{
    public void Configure(EntityTypeBuilder<RoadmapItem> b)
    {
        b.HasKey(x => x.RoadmapItemId);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        // Stored as a string, matching InputItem.Category's precedent (doc 11: enums stored as
        // strings for report/DB readability) rather than the default int.
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.Property(x => x.TargetPhase).HasMaxLength(50);
        b.HasIndex(x => x.SortOrder);
    }
}

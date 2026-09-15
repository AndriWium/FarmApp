using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class HarvestConfiguration : IEntityTypeConfiguration<Harvest>
{
    public void Configure(EntityTypeBuilder<Harvest> b)
    {
        b.HasKey(x => x.HarvestId);
        b.Property(x => x.Notes).HasMaxLength(500);

        // Not folded into Notes - a dedicated, queryable field for food-safety review (doc 05 §5,
        // task brief).
        b.Property(x => x.WithholdingOverrideReason).HasMaxLength(500);

        b.HasIndex(x => x.SeasonId);
    }
}

using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class PlantingConfiguration : IEntityTypeConfiguration<Planting>
{
    public void Configure(EntityTypeBuilder<Planting> b)
    {
        b.HasKey(x => x.PlantingId);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(500);

        b.HasIndex(x => x.BlockId);
        b.HasIndex(x => x.CultivarId);
    }
}

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

        b.HasIndex(x => x.PlantingId);
    }
}

using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class CultivarConfiguration : IEntityTypeConfiguration<Cultivar>
{
    public void Configure(EntityTypeBuilder<Cultivar> b)
    {
        b.HasKey(x => x.CultivarId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        // Scoped uniqueness: two different crops may each have a cultivar of the same name.
        b.HasIndex(x => new { x.CropId, x.Name }).IsUnique();
        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> b)
    {
        b.HasKey(x => x.BlockId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.AreaHectare).HasPrecision(18, 3);
        b.Property(x => x.Note).HasMaxLength(500);
        b.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class RainfallLogConfiguration : IEntityTypeConfiguration<RainfallLog>
{
    public void Configure(EntityTypeBuilder<RainfallLog> b)
    {
        b.HasKey(x => x.RainfallLogId);
        b.Property(x => x.Date).HasColumnType("date");
        b.Property(x => x.Mm).HasPrecision(18, 3);
        b.Property(x => x.Notes).HasMaxLength(500);

        // One reading per day - the natural-key rule doc 02 implies, enforced at the DB level.
        b.HasIndex(x => x.Date).IsUnique();
    }
}

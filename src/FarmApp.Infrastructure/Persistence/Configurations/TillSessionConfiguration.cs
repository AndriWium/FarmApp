using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class TillSessionConfiguration : IEntityTypeConfiguration<TillSession>
{
    public void Configure(EntityTypeBuilder<TillSession> b)
    {
        b.HasKey(x => x.TillSessionId);

        b.Property(x => x.SystemCardTotal).HasPrecision(18, 2);
        b.Property(x => x.CardMachineBatchTotal).HasPrecision(18, 2);
        b.Property(x => x.Difference).HasPrecision(18, 2);
        b.Property(x => x.DifferenceNote).HasMaxLength(500);

        // Supports HasOpenSessionAsync's "is there already an open session for this location" check.
        b.HasIndex(x => new { x.LocationId, x.ClosedAt });
    }
}

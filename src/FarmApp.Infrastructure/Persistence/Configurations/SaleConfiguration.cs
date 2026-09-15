using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> b)
    {
        b.HasKey(x => x.SaleId);

        b.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(500);

        // The idempotency key (doc 08): a retried POST with the same ClientGuid must never be
        // able to create a second Sale, even under a race - enforced at the DB level, not just
        // by SaleService's own "check first" read.
        b.HasIndex(x => x.ClientGuid).IsUnique();

        // Supports "which sales happened in this till session" (GET /api/v1/sales?tillSessionId=).
        b.HasIndex(x => x.TillSessionId);
    }
}

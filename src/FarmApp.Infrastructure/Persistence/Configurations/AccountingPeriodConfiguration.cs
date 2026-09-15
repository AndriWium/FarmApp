using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> b)
    {
        b.HasKey(x => x.AccountingPeriodId);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.ClosedBy).HasMaxLength(50);
        b.Property(x => x.ReopenReason).HasMaxLength(500);
        b.HasIndex(x => new { x.Year, x.Month }).IsUnique();
    }
}

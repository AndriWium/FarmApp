using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.HasKey(x => x.AuditLogId);
        b.Property(x => x.UserName).HasMaxLength(50);
        b.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
        b.Property(x => x.Action).HasMaxLength(20).IsRequired();
        b.HasIndex(x => new { x.EntityName, x.EntityId });
    }
}

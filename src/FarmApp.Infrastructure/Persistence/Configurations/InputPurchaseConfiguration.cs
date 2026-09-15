using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class InputPurchaseConfiguration : IEntityTypeConfiguration<InputPurchase>
{
    public void Configure(EntityTypeBuilder<InputPurchase> b)
    {
        b.HasKey(x => x.InputPurchaseId);
        b.Property(x => x.InvoiceRef).HasMaxLength(50);
        b.HasIndex(x => x.SupplierId);
    }
}

using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class RecipeLineConfiguration : IEntityTypeConfiguration<RecipeLine>
{
    public void Configure(EntityTypeBuilder<RecipeLine> b)
    {
        b.HasKey(x => x.RecipeLineId);
        b.Property(x => x.Qty).HasPrecision(18, 3);

        // A recipe shouldn't list the same ingredient twice (would silently double-count
        // cost/depletion) - blocked at the DB level, not just app validation. See DECISIONS.md.
        b.HasIndex(x => new { x.ProductId, x.InputItemId }).IsUnique();
    }
}

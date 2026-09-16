using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> b)
    {
        b.HasKey(x => x.ExpenseId);

        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.VatAmount).HasPrecision(18, 2);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.AttachmentPath).HasMaxLength(260); // Windows MAX_PATH-ish cap, plenty for a stored file path

        // Supports the income-statement/expenses-by-category reporting queries and the
        // GET /api/v1/expenses?from=&to=&categoryId= list filter.
        b.HasIndex(x => x.Date);
        b.HasIndex(x => x.ExpenseCategoryId);
        b.HasIndex(x => x.SeasonId);
    }
}

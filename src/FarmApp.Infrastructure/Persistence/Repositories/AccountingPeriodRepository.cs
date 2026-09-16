using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class AccountingPeriodRepository(FarmAppDbContext db) : IAccountingPeriodRepository
{
    public Task<AccountingPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken ct)
        => db.AccountingPeriods.FirstOrDefaultAsync(x => x.Year == year && x.Month == month, ct);

    public async Task AddAsync(AccountingPeriod period, CancellationToken ct)
        => await db.AccountingPeriods.AddAsync(period, ct);
}

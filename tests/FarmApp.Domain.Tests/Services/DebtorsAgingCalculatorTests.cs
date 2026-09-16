using FarmApp.Domain.Services;

namespace FarmApp.Domain.Tests.Services;

public class DebtorsAgingCalculatorTests
{
    private readonly DebtorsAgingCalculator _sut = new();

    [Fact]
    public void ApplyPaymentsOldestFirst_PaymentCoversOnlyOldestDebt_SettlesOldestLeavesNewerUntouched()
    {
        var debts = new[]
        {
            new DebtLine(SaleId: 1, Date: new DateTime(2026, 1, 1), Amount: 100m),
            new DebtLine(SaleId: 2, Date: new DateTime(2026, 6, 1), Amount: 50m),
        };

        var result = _sut.ApplyPaymentsOldestFirst(debts, totalPayments: 100m);

        Assert.Equal(0m, result[0].OutstandingAmount); // oldest fully settled
        Assert.Equal(50m, result[1].OutstandingAmount); // newer untouched
    }

    // The brief's own worked scenario: an old unpaid sale and a newer one, a partial payment -
    // confirm it settles the older debt first.
    [Fact]
    public void ApplyPaymentsOldestFirst_PartialPayment_SettlesOlderDebtFirstThenSpillsToNewer()
    {
        var debts = new[]
        {
            new DebtLine(SaleId: 1, Date: new DateTime(2026, 1, 1), Amount: 100m),
            new DebtLine(SaleId: 2, Date: new DateTime(2026, 8, 1), Amount: 80m),
        };

        var result = _sut.ApplyPaymentsOldestFirst(debts, totalPayments: 120m);

        Assert.Equal(0m, result[0].OutstandingAmount); // R100 of the R120 clears the old debt entirely
        Assert.Equal(60m, result[1].OutstandingAmount); // remaining R20 chips into the newer R80 debt
    }

    [Fact]
    public void ApplyPaymentsOldestFirst_NoPayments_EveryDebtFullyOutstanding()
    {
        var debts = new[]
        {
            new DebtLine(SaleId: 1, Date: new DateTime(2026, 1, 1), Amount: 100m),
            new DebtLine(SaleId: 2, Date: new DateTime(2026, 6, 1), Amount: 50m),
        };

        var result = _sut.ApplyPaymentsOldestFirst(debts, totalPayments: 0m);

        Assert.Equal(100m, result[0].OutstandingAmount);
        Assert.Equal(50m, result[1].OutstandingAmount);
    }

    [Fact]
    public void ApplyPaymentsOldestFirst_PaymentExceedsTotalDebt_EveryDebtSettledNeverNegative()
    {
        var debts = new[]
        {
            new DebtLine(SaleId: 1, Date: new DateTime(2026, 1, 1), Amount: 100m),
            new DebtLine(SaleId: 2, Date: new DateTime(2026, 6, 1), Amount: 50m),
        };

        var result = _sut.ApplyPaymentsOldestFirst(debts, totalPayments: 500m);

        Assert.All(result, d => Assert.Equal(0m, d.OutstandingAmount));
    }

    [Fact]
    public void ApplyPaymentsOldestFirst_NoDebts_ReturnsEmpty()
    {
        Assert.Empty(_sut.ApplyPaymentsOldestFirst(Array.Empty<DebtLine>(), totalPayments: 100m));
    }

    [Fact]
    public void Bucket_29DaysOld_IsCurrent_30DaysOld_IsDays30()
    {
        var asOf = new DateTime(2026, 9, 16);

        Assert.Equal(100m, _sut.Bucket([new OutstandingDebt(1, asOf.AddDays(-29), 100m)], asOf).Current);
        Assert.Equal(100m, _sut.Bucket([new OutstandingDebt(1, asOf.AddDays(-30), 100m)], asOf).Days30);
    }

    [Fact]
    public void Bucket_59DaysOld_IsDays30_60DaysOld_IsDays60()
    {
        var asOf = new DateTime(2026, 9, 16);

        Assert.Equal(100m, _sut.Bucket([new OutstandingDebt(1, asOf.AddDays(-59), 100m)], asOf).Days30);
        Assert.Equal(100m, _sut.Bucket([new OutstandingDebt(1, asOf.AddDays(-60), 100m)], asOf).Days60);
    }

    [Fact]
    public void Bucket_89DaysOld_IsDays60_90DaysOld_IsDays90Plus()
    {
        var asOf = new DateTime(2026, 9, 16);

        Assert.Equal(100m, _sut.Bucket([new OutstandingDebt(1, asOf.AddDays(-89), 100m)], asOf).Days60);
        Assert.Equal(100m, _sut.Bucket([new OutstandingDebt(1, asOf.AddDays(-90), 100m)], asOf).Days90Plus);
    }

    [Fact]
    public void Bucket_AllFourAgeBands_EachLandsInItsOwnBucket()
    {
        var asOf = new DateTime(2026, 9, 16);
        var debts = new[]
        {
            new OutstandingDebt(1, asOf.AddDays(-5), 10m),   // Current
            new OutstandingDebt(2, asOf.AddDays(-45), 20m),  // Days30
            new OutstandingDebt(3, asOf.AddDays(-75), 30m),  // Days60
            new OutstandingDebt(4, asOf.AddDays(-120), 40m), // Days90Plus
        };

        var result = _sut.Bucket(debts, asOf);

        Assert.Equal(10m, result.Current);
        Assert.Equal(20m, result.Days30);
        Assert.Equal(30m, result.Days60);
        Assert.Equal(40m, result.Days90Plus);
        Assert.Equal(100m, result.Total);
    }

    [Fact]
    public void Bucket_FullySettledDebt_ContributesNothing()
    {
        var asOf = new DateTime(2026, 9, 16);
        var debts = new[] { new OutstandingDebt(1, asOf.AddDays(-100), OutstandingAmount: 0m) };

        var result = _sut.Bucket(debts, asOf);

        Assert.Equal(0m, result.Total);
    }

    // End-to-end version of the brief's own verification scenario: an old sale, a newer sale, one
    // partial payment - confirm it settles the older debt first AND buckets correctly afterwards.
    [Fact]
    public void EndToEnd_OldSaleNewSalePartialPayment_SettlesOldestFirstAndBucketsRemainderCorrectly()
    {
        var asOf = new DateTime(2026, 9, 16);
        var debts = new[]
        {
            new DebtLine(SaleId: 1, Date: asOf.AddDays(-100), Amount: 100m), // old - would be Days90Plus if unpaid
            new DebtLine(SaleId: 2, Date: asOf.AddDays(-10), Amount: 80m),   // newer - would be Current if unpaid
        };

        var outstanding = _sut.ApplyPaymentsOldestFirst(debts, totalPayments: 120m);
        var buckets = _sut.Bucket(outstanding, asOf);

        // R120 payment: R100 clears the old debt entirely, remaining R20 reduces the newer R80
        // debt to R60 - the old debt contributes nothing to Days90Plus (fully settled), and the
        // newer debt's R60 remainder lands in Current (it's still only 10 days old).
        Assert.Equal(0m, buckets.Days90Plus);
        Assert.Equal(60m, buckets.Current);
        Assert.Equal(60m, buckets.Total);
    }
}

namespace FarmApp.Domain.Services;

/// <summary>One Account-method sale debt for a customer, before any payment is applied - a plain
/// data-carrier, not the Sale entity itself (this calculator has zero dependencies, doc 11).</summary>
public record DebtLine(int SaleId, DateTime Date, decimal Amount);

/// <summary>One debt's remaining balance after a customer's payments have been applied oldest-
/// first. OutstandingAmount is always &gt;= 0 - a payment can fully settle a debt but never drive
/// it negative (any leftover after fully settling one debt rolls forward to the next-oldest).</summary>
public record OutstandingDebt(int SaleId, DateTime Date, decimal OutstandingAmount);

/// <summary>One customer's outstanding balance bucketed by age from a given as-of date (doc 04
/// §5's "current / 30 / 60 / 90+"). Total is a convenience, not an independently-set value.</summary>
public record DebtorAgingBuckets(decimal Current, decimal Days30, decimal Days60, decimal Days90Plus)
{
    public decimal Total => Current + Days30 + Days60 + Days90Plus;
}

/// <summary>Pure arithmetic for doc 04 §5's debtors aging (task brief): CustomerPayment doesn't
/// reference a specific Sale, so exact invoice-level aging isn't directly computable from the
/// schema. The standard, legitimate simplification this implements - apply a customer's total
/// payments against their Account-method sales oldest-first, then bucket whatever remains
/// unsettled by each contributing sale's age - is exactly the kind of pure "given these debts and
/// this payment total, which debts are how-settled" arithmetic doc 11 puts in Domain (same shape
/// as SaleLineCalculator/SeasonCostCalculator). Zero dependencies - no EF, no database, no I/O.</summary>
public interface IDebtorsAgingCalculator
{
    /// <summary>Applies totalPayments against debtsOldestFirst in the order given - the caller
    /// (ReportQueries) is responsible for actually sorting oldest-first; this trusts that
    /// ordering rather than re-deriving it, keeping the calculator itself trivial and testable
    /// independent of any date-comparison concern. A payment amount exceeding the total debt
    /// leaves every debt at zero (never negative) rather than producing a customer credit row -
    /// doc 04 §5 has no notion of a customer credit balance to carry forward.</summary>
    IReadOnlyList<OutstandingDebt> ApplyPaymentsOldestFirst(IReadOnlyList<DebtLine> debtsOldestFirst, decimal totalPayments);

    /// <summary>Buckets by whole days between asOf and each debt's Date - Current is age &lt; 30,
    /// Days30 is 30-59, Days60 is 60-89, Days90Plus is 90+. Debts with OutstandingAmount == 0
    /// (fully settled) contribute nothing to any bucket, which is what makes a customer with no
    /// unpaid balance simply not show up in an aging report built from this.</summary>
    DebtorAgingBuckets Bucket(IReadOnlyList<OutstandingDebt> outstanding, DateTime asOf);
}

public class DebtorsAgingCalculator : IDebtorsAgingCalculator
{
    public IReadOnlyList<OutstandingDebt> ApplyPaymentsOldestFirst(IReadOnlyList<DebtLine> debtsOldestFirst, decimal totalPayments)
    {
        ArgumentNullException.ThrowIfNull(debtsOldestFirst);

        var remaining = Math.Max(totalPayments, 0m);
        var result = new List<OutstandingDebt>(debtsOldestFirst.Count);

        foreach (var debt in debtsOldestFirst)
        {
            var applied = Math.Min(debt.Amount, remaining);
            remaining -= applied;
            result.Add(new OutstandingDebt(debt.SaleId, debt.Date, debt.Amount - applied));
        }

        return result;
    }

    public DebtorAgingBuckets Bucket(IReadOnlyList<OutstandingDebt> outstanding, DateTime asOf)
    {
        ArgumentNullException.ThrowIfNull(outstanding);

        decimal current = 0, days30 = 0, days60 = 0, days90Plus = 0;
        foreach (var debt in outstanding)
        {
            if (debt.OutstandingAmount <= 0) continue;

            var ageDays = (asOf.Date - debt.Date.Date).Days;
            if (ageDays < 30) current += debt.OutstandingAmount;
            else if (ageDays < 60) days30 += debt.OutstandingAmount;
            else if (ageDays < 90) days60 += debt.OutstandingAmount;
            else days90Plus += debt.OutstandingAmount;
        }

        return new DebtorAgingBuckets(current, days30, days60, days90Plus);
    }
}

using System.Data;
using Dapper;
using FarmApp.Domain.Services;
using Microsoft.Data.SqlClient;

namespace FarmApp.Api.Application.Reports;

/// <summary>The one deliberate exception to the codebase's usual entity -> repository -> service
/// layering (doc 11): straight Dapper/raw SQL against the Reporting schema's views, no
/// IReportRepository abstraction, no IUnitOfWork. A plain concrete class (not
/// interface+implementation like every other Application service) - reports are read-only
/// aggregation queries, not business logic with a second implementation ever likely to exist, so
/// the usual DI-for-testability ceremony isn't buying anything here (see DECISIONS.md).
///
/// Constructor-injects IDebtorsAgingCalculator alongside IConfiguration (Phase 4c) - the one
/// place a report needs real Domain arithmetic rather than a SQL aggregation: debtors aging
/// applies a customer's payments against their debts oldest-first (a pure calculation, unit
/// tested independently), which Dapper fetches the raw rows for but shouldn't compute inline as
/// a SQL CASE/window-function pile.</summary>
public class ReportQueries(IConfiguration configuration, IDebtorsAgingCalculator agingCalculator)
{
    private string ConnectionString => configuration.GetConnectionString("FarmApp")
        ?? throw new InvalidOperationException("Missing 'FarmApp' connection string.");

    private IDbConnection CreateConnection() => new SqlConnection(ConnectionString);

    public async Task<IReadOnlyList<SalesAnalysisRowDto>> GetSalesAnalysisAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        using var db = CreateConnection();
        var rows = await db.QueryAsync<SalesAnalysisRowDto>(new CommandDefinition(
            """
            SELECT SaleLineId, SaleId, SaleDateTime, Channel, CustomerId,
                   ProductId, ProductName, GradeId, GradeName, PackSizeId, PackSizeName,
                   Qty, UnitPrice, DiscountAmount, DiscountReason, CostAtSale, LineTotal, GrossMargin
            FROM Reporting.SalesAnalysis
            WHERE SaleDateTime >= @from AND SaleDateTime <= @to
            ORDER BY SaleDateTime;
            """,
            new { from, to }, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<StockOnHandDto>> GetStockOnHandAsync(CancellationToken ct)
    {
        using var db = CreateConnection();
        var rows = await db.QueryAsync<StockOnHandDto>(new CommandDefinition(
            """
            SELECT ProductId, ProductName, GradeId, GradeName, QtyOnHand, Value
            FROM Reporting.StockOnHand
            ORDER BY ProductName, GradeName;
            """,
            cancellationToken: ct));
        return rows.AsList();
    }

    /// <summary>Reporting.StockMovementSummary is detail grain (a SQL Server view can't take a
    /// date-range parameter) - the opening/in/out/closing split is computed here, parameterised
    /// by @from/@to, per Product/Grade (optionally narrowed to one).</summary>
    public async Task<IReadOnlyList<StockMovementSummaryDto>> GetStockMovementSummaryAsync(
        DateTime from, DateTime to, int? productId, int? gradeId, CancellationToken ct)
    {
        using var db = CreateConnection();
        var rows = await db.QueryAsync<StockMovementSummaryDto>(new CommandDefinition(
            """
            SELECT
                ProductId, ProductName, GradeId, GradeName,
                SUM(CASE WHEN [Date] < @from THEN Qty ELSE 0 END)                            AS OpeningBalance,
                SUM(CASE WHEN [Date] >= @from AND [Date] <= @to AND Qty > 0 THEN Qty ELSE 0 END)  AS TotalIn,
                SUM(CASE WHEN [Date] >= @from AND [Date] <= @to AND Qty < 0 THEN -Qty ELSE 0 END) AS TotalOut,
                SUM(CASE WHEN [Date] <= @to THEN Qty ELSE 0 END)                              AS ClosingBalance
            FROM Reporting.StockMovementSummary
            WHERE (@productId IS NULL OR ProductId = @productId)
              AND (@gradeId IS NULL OR GradeId = @gradeId)
            GROUP BY ProductId, ProductName, GradeId, GradeName
            ORDER BY ProductName, GradeName;
            """,
            new { from, to, productId, gradeId }, cancellationToken: ct));
        return rows.AsList();
    }

    /// <summary>Assembles doc 04 §1's income statement from three independent queries (Sales/Cos
    /// from SalesAnalysis, Wastage from WastageValue, expenses from ExpensesByCategory) rather
    /// than one combined view - each source has a different grain/join shape, and combining them
    /// in SQL would need UNION ALL gymnastics for no real benefit over summing three small result
    /// sets here.</summary>
    public async Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        using var db = CreateConnection();

        var salesCos = await db.QuerySingleAsync<SalesCosRow>(new CommandDefinition(
            """
            SELECT ISNULL(SUM(LineTotal), 0) AS Sales, ISNULL(SUM(CostAtSale * Qty), 0) AS Cos
            FROM Reporting.SalesAnalysis
            WHERE SaleDateTime >= @from AND SaleDateTime <= @to;
            """,
            new { from, to }, cancellationToken: ct));

        var wastage = await db.QuerySingleAsync<decimal>(new CommandDefinition(
            "SELECT ISNULL(SUM(Value), 0) FROM Reporting.WastageValue WHERE [Date] >= @from AND [Date] <= @to;",
            new { from, to }, cancellationToken: ct));

        var expensesByCategory = (await db.QueryAsync<ExpenseCategoryTotalDto>(new CommandDefinition(
            """
            SELECT ExpenseCategoryId, CategoryName, SUM(Amount) AS Total
            FROM Reporting.ExpensesByCategory
            WHERE [Date] >= @from AND [Date] <= @to
            GROUP BY ExpenseCategoryId, CategoryName
            ORDER BY CategoryName;
            """,
            new { from, to }, cancellationToken: ct))).AsList();

        var totalExpenses = expensesByCategory.Sum(x => x.Total);
        var grossProfit = salesCos.Sales - salesCos.Cos - wastage;
        var netProfit = grossProfit - totalExpenses;

        return new IncomeStatementDto(salesCos.Sales, salesCos.Cos, grossProfit, wastage, expensesByCategory, totalExpenses, netProfit);
    }

    private record SalesCosRow(decimal Sales, decimal Cos);

    // ---- Farming report (doc 04 §4, Phase 4c) ----

    /// <summary>doc 04 §4's revenue-attribution approximation. SaleLine (price) and StockMovement
    /// (batch, hence season, via StockBatch.HarvestId -> Harvest.SeasonId) aren't directly linked
    /// by a shared key - a sale line never records which batch(es) FIFO depletion actually
    /// touched. This allocates the period's total sales revenue for each Product/Grade
    /// proportionally by how much of that quantity was depleted from THIS season's harvest
    /// batches vs. all sources in the same window (Reporting.SaleOutDepletion, grouped both with
    /// and without the SeasonId filter) - a reasonable approximation for the "should I plant this
    /// again?" table, not an audited figure (see DECISIONS.md). ISNULL(GradeId, 0) sidesteps
    /// SQL's NULL <> NULL when joining on a nullable grade.</summary>
    public async Task<decimal> GetSeasonRevenueAttributedAsync(int seasonId, DateTime from, DateTime to, CancellationToken ct)
    {
        using var db = CreateConnection();
        var result = await db.QuerySingleAsync<decimal?>(new CommandDefinition(
            """
            WITH SeasonDepletion AS (
                SELECT ProductId, GradeId, SUM(QtyDepleted) AS SeasonQty
                FROM Reporting.SaleOutDepletion
                WHERE SeasonId = @seasonId AND [Date] >= @from AND [Date] <= @to
                GROUP BY ProductId, GradeId
            ),
            TotalDepletion AS (
                SELECT ProductId, GradeId, SUM(QtyDepleted) AS TotalQty
                FROM Reporting.SaleOutDepletion
                WHERE [Date] >= @from AND [Date] <= @to
                GROUP BY ProductId, GradeId
            ),
            Revenue AS (
                SELECT ProductId, GradeId, SUM(LineTotal) AS TotalRevenue
                FROM Reporting.SalesAnalysis
                WHERE SaleDateTime >= @from AND SaleDateTime <= @to
                GROUP BY ProductId, GradeId
            )
            SELECT SUM(r.TotalRevenue * sd.SeasonQty / NULLIF(td.TotalQty, 0))
            FROM SeasonDepletion sd
            JOIN TotalDepletion td ON td.ProductId = sd.ProductId AND ISNULL(td.GradeId, 0) = ISNULL(sd.GradeId, 0)
            JOIN Revenue r ON r.ProductId = sd.ProductId AND ISNULL(r.GradeId, 0) = ISNULL(sd.GradeId, 0);
            """,
            new { seasonId, from, to }, cancellationToken: ct));
        return result ?? 0m;
    }

    /// <summary>doc 04 §4's harvest summary: this season's kg per product/grade vs. the same
    /// crop's most recent earlier season. Two round trips rather than one window-function query
    /// (see db/views/Reporting.HarvestSummaryBySeason.sql) - simple beats clever for a report this
    /// infrequently run.</summary>
    public async Task<IReadOnlyList<HarvestSummaryRowDto>> GetHarvestSummaryAsync(int seasonId, CancellationToken ct)
    {
        using var db = CreateConnection();

        var current = (await db.QueryAsync<HarvestSummaryRow>(new CommandDefinition(
            """
            SELECT CropId, StartDate, ProductId, ProductName, GradeId, GradeName, QtyKg
            FROM Reporting.HarvestSummaryBySeason
            WHERE SeasonId = @seasonId;
            """,
            new { seasonId }, cancellationToken: ct))).AsList();

        if (current.Count == 0) return [];

        var cropId = current[0].CropId;
        var startDate = current[0].StartDate;

        var priorSeasonId = await db.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
            """
            SELECT TOP 1 s2.SeasonId
            FROM dbo.Seasons s2
            JOIN dbo.Plantings pl2 ON pl2.PlantingId = s2.PlantingId
            JOIN dbo.Cultivars cv2 ON cv2.CultivarId = pl2.CultivarId
            WHERE cv2.CropId = @cropId AND s2.StartDate < @startDate
            ORDER BY s2.StartDate DESC;
            """,
            new { cropId, startDate }, cancellationToken: ct));

        List<HarvestSummaryRow> prior = priorSeasonId is null
            ? []
            : (await db.QueryAsync<HarvestSummaryRow>(new CommandDefinition(
                """
                SELECT CropId, StartDate, ProductId, ProductName, GradeId, GradeName, QtyKg
                FROM Reporting.HarvestSummaryBySeason
                WHERE SeasonId = @priorSeasonId;
                """,
                new { priorSeasonId }, cancellationToken: ct))).AsList();

        // Nullable decimal? value type is deliberate here (not plain decimal) - Dictionary<,>
        // .GetValueOrDefault on a non-nullable decimal would silently return 0 for "no prior
        // season row for this product/grade", indistinguishable from "prior season genuinely
        // harvested zero" - the whole point of this column is telling "no prior season exists"
        // apart from "it did, but harvested nothing" (task brief: "vs last season, if one
        // exists").
        var priorByKey = prior.ToDictionary(x => (x.ProductId, x.GradeId), x => (decimal?)x.QtyKg);

        return current
            .Select(c => new HarvestSummaryRowDto(
                c.ProductId, c.ProductName, c.GradeId, c.GradeName,
                c.QtyKg, priorByKey.GetValueOrDefault((c.ProductId, c.GradeId))))
            .ToList();
    }

    /// <summary>doc 04 §4's input usage summary - optional seasonId and/or from/to filters, summed
    /// per InputItem (matches GetStockMovementSummaryAsync's own "optional narrowing filter,
    /// grouped by the caller" shape).</summary>
    public async Task<IReadOnlyList<InputUsageRowDto>> GetInputUsageAsync(
        int? seasonId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        using var db = CreateConnection();
        var rows = await db.QueryAsync<InputUsageRowDto>(new CommandDefinition(
            """
            SELECT InputItemId, InputItemName, SUM(Qty) AS QtyUsed, SUM(Cost) AS Cost
            FROM Reporting.InputUsageBySeason
            WHERE (@seasonId IS NULL OR SeasonId = @seasonId)
              AND (@from IS NULL OR [Date] >= @from)
              AND (@to IS NULL OR [Date] <= @to)
            GROUP BY InputItemId, InputItemName
            ORDER BY InputItemName;
            """,
            new { seasonId, from, to }, cancellationToken: ct));
        return rows.AsList();
    }

    /// <summary>doc 04 §4's "rainfall for the month vs historical" - this period's total against
    /// the average of the same calendar month across every OTHER year with a reading. Straight
    /// against dbo.RainfallLogs (no view - a single-table aggregation needs no join).</summary>
    public async Task<RainfallComparisonDto> GetRainfallComparisonAsync(int year, int month, CancellationToken ct)
    {
        using var db = CreateConnection();

        var thisPeriod = await db.QuerySingleAsync<decimal>(new CommandDefinition(
            """
            SELECT ISNULL(SUM(Mm), 0) FROM dbo.RainfallLogs
            WHERE YEAR([Date]) = @year AND MONTH([Date]) = @month;
            """,
            new { year, month }, cancellationToken: ct));

        var otherYears = (await db.QueryAsync<decimal>(new CommandDefinition(
            """
            SELECT SUM(Mm) AS MonthlyTotal
            FROM dbo.RainfallLogs
            WHERE MONTH([Date]) = @month AND YEAR([Date]) <> @year
            GROUP BY YEAR([Date]);
            """,
            new { year, month }, cancellationToken: ct))).AsList();

        decimal? historicalAverage = otherYears.Count > 0 ? otherYears.Average() : null;

        return new RainfallComparisonDto(year, month, thisPeriod, historicalAverage, otherYears.Count);
    }

    // ---- Cash & debtors report (doc 04 §5, Phase 4c) ----

    /// <summary>doc 04 §5's till sessions over/short for a date range (filtered on ClosedAt, so a
    /// session only shows once its day close has actually run). Orderable/groupable by OpenedBy
    /// at the calling layer - no server-side grouping here, matching SalesAnalysis's own
    /// "detail grain, let the caller group" precedent.</summary>
    public async Task<IReadOnlyList<TillSessionOverShortDto>> GetTillSessionsAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        using var db = CreateConnection();
        var rows = await db.QueryAsync<TillSessionOverShortDto>(new CommandDefinition(
            """
            SELECT TillSessionId, LocationId, OpenedAt, OpenedBy, ClosedAt,
                   SystemCardTotal, CardMachineBatchTotal, Difference, DifferenceNote
            FROM Reporting.TillSessionsOverShort
            WHERE ClosedAt >= @from AND ClosedAt <= @to
            ORDER BY OpenedBy, ClosedAt;
            """,
            new { from, to }, cancellationToken: ct));
        return rows.AsList();
    }

    /// <summary>doc 04 §5's cash flow summary, assembled from independent queries (same pattern as
    /// GetIncomeStatementAsync) rather than one combined view - each source has a different join
    /// shape. Cash in = card/EFT sale payments + all customer debt payments; cash out = expenses +
    /// produce/input purchase costs (task brief's own breakdown). Named "cash flow" per doc 04's
    /// wording, but every figure here is card/EFT/account movement, not literal cash (doc 01 §4's
    /// card-only decision) - see CashFlowDto/DECISIONS.md.</summary>
    public async Task<CashFlowDto> GetCashFlowAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        using var db = CreateConnection();

        var cashInCardEft = await db.QuerySingleAsync<decimal>(new CommandDefinition(
            """
            SELECT ISNULL(SUM(Amount), 0) FROM Reporting.SalePaymentDetail
            WHERE Method IN ('Card', 'EFT') AND SaleDateTime >= @from AND SaleDateTime <= @to;
            """,
            new { from, to }, cancellationToken: ct));

        var cashInDebtorPayments = await db.QuerySingleAsync<decimal>(new CommandDefinition(
            "SELECT ISNULL(SUM(Amount), 0) FROM dbo.CustomerPayments WHERE [Date] >= @from AND [Date] <= @to;",
            new { from, to }, cancellationToken: ct));

        var cashOutExpenses = await db.QuerySingleAsync<decimal>(new CommandDefinition(
            "SELECT ISNULL(SUM(Amount), 0) FROM Reporting.ExpensesByCategory WHERE [Date] >= @from AND [Date] <= @to;",
            new { from, to }, cancellationToken: ct));

        var cashOutPurchases = await db.QuerySingleAsync<decimal>(new CommandDefinition(
            "SELECT ISNULL(SUM(Amount), 0) FROM Reporting.PurchaseCosts WHERE [Date] >= @from AND [Date] <= @to;",
            new { from, to }, cancellationToken: ct));

        var cashIn = cashInCardEft + cashInDebtorPayments;
        var cashOut = cashOutExpenses + cashOutPurchases;

        return new CashFlowDto(cashInCardEft, cashInDebtorPayments, cashIn, cashOutExpenses, cashOutPurchases, cashOut, cashIn - cashOut);
    }

    /// <summary>doc 04 §5's debtors aging: apply each customer's total CustomerPayments against
    /// their Account-method sales oldest-first (the standard simplification the task brief
    /// describes - CustomerPayment doesn't reference a specific Sale), then bucket whatever
    /// remains by age from today. The actual allocation/bucketing arithmetic is
    /// IDebtorsAgingCalculator (Domain, unit tested) - this method's job is only fetching the raw
    /// per-customer debt/payment rows and handing them over. Only customers with a non-zero
    /// outstanding total are returned.</summary>
    public async Task<IReadOnlyList<DebtorAgingRowDto>> GetDebtorsAgingAsync(CancellationToken ct)
    {
        using var db = CreateConnection();

        var debtRows = (await db.QueryAsync<AccountSaleRow>(new CommandDefinition(
            """
            SELECT a.CustomerId, c.Name AS CustomerName, a.SaleId, a.SaleDate, a.Amount
            FROM Reporting.AccountSalesDetail a
            JOIN dbo.Customers c ON c.CustomerId = a.CustomerId
            ORDER BY a.CustomerId, a.SaleDate;
            """,
            cancellationToken: ct))).AsList();

        var paymentsByCustomer = (await db.QueryAsync<(int CustomerId, decimal Total)>(new CommandDefinition(
            "SELECT CustomerId, SUM(Amount) AS Total FROM dbo.CustomerPayments GROUP BY CustomerId;",
            cancellationToken: ct)))
            .ToDictionary(x => x.CustomerId, x => x.Total);

        var asOf = DateTime.Today;
        var result = new List<DebtorAgingRowDto>();

        foreach (var group in debtRows.GroupBy(x => (x.CustomerId, x.CustomerName)))
        {
            var debts = group
                .OrderBy(x => x.SaleDate) // oldest-first, per IDebtorsAgingCalculator's contract
                .Select(x => new DebtLine(x.SaleId, x.SaleDate, x.Amount))
                .ToList();
            var totalPaid = paymentsByCustomer.GetValueOrDefault(group.Key.CustomerId);

            var outstanding = agingCalculator.ApplyPaymentsOldestFirst(debts, totalPaid);
            var buckets = agingCalculator.Bucket(outstanding, asOf);

            if (buckets.Total <= 0) continue; // fully settled - nothing to show on an aging report

            result.Add(new DebtorAgingRowDto(
                group.Key.CustomerId, group.Key.CustomerName,
                buckets.Current, buckets.Days30, buckets.Days60, buckets.Days90Plus, buckets.Total));
        }

        return result.OrderByDescending(x => x.Total).ToList();
    }

    private record AccountSaleRow(int CustomerId, string CustomerName, int SaleId, DateTime SaleDate, decimal Amount);

    /// <summary>Every Wastage-type movement whose Date falls in the given [year, month] - the
    /// month-end close checklist's informational item 5 (doc 10 §1). Reuses
    /// Reporting.WastageValue (Phase 4b) rather than a new view - same underlying data, just
    /// period-filtered instead of date-range-filtered.</summary>
    public async Task<IReadOnlyList<WastageEntryRowDto>> GetWastageInPeriodAsync(int year, int month, CancellationToken ct)
    {
        using var db = CreateConnection();
        var periodStart = new DateTime(year, month, 1);
        var periodEnd = periodStart.AddMonths(1);
        var rows = await db.QueryAsync<WastageEntryRowDto>(new CommandDefinition(
            """
            SELECT StockMovementId, [Date], ProductId, GradeId, Qty, UnitCost, Value
            FROM Reporting.WastageValue
            WHERE [Date] >= @periodStart AND [Date] < @periodEnd
            ORDER BY [Date];
            """,
            new { periodStart, periodEnd }, cancellationToken: ct));
        return rows.AsList();
    }

    private record HarvestSummaryRow(
        int CropId, DateTime StartDate, int ProductId, string ProductName, int? GradeId, string? GradeName, decimal QtyKg);
}

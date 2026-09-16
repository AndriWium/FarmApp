using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace FarmApp.Api.Application.Reports;

/// <summary>The one deliberate exception to the codebase's usual entity -> repository -> service
/// layering (doc 11): straight Dapper/raw SQL against the Reporting schema's views, no
/// IReportRepository abstraction, no IUnitOfWork. A plain concrete class (not
/// interface+implementation like every other Application service) - reports are read-only
/// aggregation queries, not business logic with a second implementation ever likely to exist, so
/// the usual DI-for-testability ceremony isn't buying anything here (see DECISIONS.md).</summary>
public class ReportQueries(IConfiguration configuration)
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
}

namespace FarmApp.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);

    /// <summary>Begins a real database transaction wrapping every write until
    /// CommitTransactionAsync/RollbackTransactionAsync (Phase 2a task brief) - unlike
    /// ProducePurchaseService/StockBatchService/StockTakeService's older two-phase-save stopgap
    /// (sequential SaveChangesAsync calls with only pre-validation narrowing the failure window,
    /// see DECISIONS.md), a mid-operation failure after this point leaves nothing behind, not
    /// even an already-flushed SaveChangesAsync from earlier in the same operation. Deliberately
    /// returns/takes nothing EF-specific (no IDbContextTransaction) so this interface stays safe
    /// for Domain/Application code to depend on without pulling EF Core into either layer -
    /// FarmAppDbContext is the only implementation and owns the actual transaction object.</summary>
    Task BeginTransactionAsync(CancellationToken ct);
    Task CommitTransactionAsync(CancellationToken ct);
    Task RollbackTransactionAsync(CancellationToken ct);
}

using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FarmApp.Infrastructure.Persistence;

public class FarmAppDbContext(DbContextOptions<FarmAppDbContext> options) : DbContext(options), IUnitOfWork //understand what IUnitOfWork does for us here
{
    // Backing field for IUnitOfWork's Begin/Commit/RollbackTransactionAsync (Phase 2a) - the one
    // EF-specific object behind that abstraction; Domain/Application code never sees this type.
    private IDbContextTransaction? _currentTransaction;

    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<Cultivar> Cultivars => Set<Cultivar>();
    public DbSet<InputItem> InputItems => Set<InputItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PackSize> PackSizes => Set<PackSize>();
    public DbSet<RecipeLine> RecipeLines => Set<RecipeLine>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Price> Prices => Set<Price>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ProducePurchase> ProducePurchases => Set<ProducePurchase>();
    public DbSet<ProducePurchaseLine> ProducePurchaseLines => Set<ProducePurchaseLine>();
    public DbSet<StockTake> StockTakes => Set<StockTake>();
    public DbSet<StockTakeLine> StockTakeLines => Set<StockTakeLine>();
    public DbSet<TillSession> TillSessions => Set<TillSession>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(FarmAppDbContext).Assembly);
    }

    public async Task BeginTransactionAsync(CancellationToken ct)
        => _currentTransaction = await Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction to commit.");
        try
        {
            await _currentTransaction.CommitAsync(ct);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct)
    {
        if (_currentTransaction is null) return; // nothing open - safe no-op, callers rollback unconditionally in catch blocks
        try
        {
            await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
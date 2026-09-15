using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence;

public class FarmAppDbContext(DbContextOptions<FarmAppDbContext> options) : DbContext(options), IUnitOfWork //understand what IUnitOfWork does for us here
{
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

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(FarmAppDbContext).Assembly);
    }
}
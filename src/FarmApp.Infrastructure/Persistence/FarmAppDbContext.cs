using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence;

public class FarmAppDbContext(DbContextOptions<FarmAppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Crop> Crops => Set<Crop>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(FarmAppDbContext).Assembly);
    }
}
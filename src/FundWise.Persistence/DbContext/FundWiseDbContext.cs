using Microsoft.EntityFrameworkCore;
using FundWise.Domain.Entities;

namespace FundWise.Persistence.DbContext;

public sealed class FundWiseDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public FundWiseDbContext(DbContextOptions<FundWiseDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<MerchantCategoryMemory> MerchantCategoryMemories => Set<MerchantCategoryMemory>();
    public DbSet<AlternativeProduct> AlternativeProducts => Set<AlternativeProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FundWiseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

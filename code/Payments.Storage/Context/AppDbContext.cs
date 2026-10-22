using Microsoft.EntityFrameworkCore;
using Payments.Storage.Models;

namespace Payments.Storage.Context;

public class AppDbContext : DbContext
{
    public DbSet<PaymentDb> Payments { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

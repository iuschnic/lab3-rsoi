using Microsoft.EntityFrameworkCore;
using Rentals.Storage.Models;

namespace Rentals.Storage.Context;

public class AppDbContext : DbContext
{
    public DbSet<RentalDb> Rentals { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

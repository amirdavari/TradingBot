using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data;

/// <summary>
/// Database context for AI Broker MVP.
/// Uses SQLite for simplicity.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WatchlistSymbol> WatchlistSymbols { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure WatchlistSymbol
        modelBuilder.Entity<WatchlistSymbol>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Symbol).IsUnique();
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}

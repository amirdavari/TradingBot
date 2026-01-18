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
    public DbSet<PaperTrade> PaperTrades { get; set; }
    public DbSet<TradeHistory> TradeHistory { get; set; }
    public DbSet<ReplayStateEntity> ReplayStates { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<RiskSettingsEntity> RiskSettings { get; set; }

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

        // Configure PaperTrade
        modelBuilder.Entity<PaperTrade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Direction).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.EntryPrice).HasPrecision(18, 2);
            entity.Property(e => e.StopLoss).HasPrecision(18, 2);
            entity.Property(e => e.TakeProfit).HasPrecision(18, 2);
            entity.Property(e => e.PositionSize).HasPrecision(18, 4);
            entity.Property(e => e.InvestAmount).HasPrecision(18, 2);
            entity.Property(e => e.ExitPrice).HasPrecision(18, 2);
            entity.Property(e => e.PnL).HasPrecision(18, 2);
            entity.Property(e => e.PnLPercent).HasPrecision(18, 4);
            entity.Property(e => e.OpenedAt).IsRequired();
            
            // Store Reasons as JSON string
            entity.Property(e => e.Reasons)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                );
            
            entity.HasIndex(e => e.Symbol);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OpenedAt);
        });

        // Configure TradeHistory
        modelBuilder.Entity<TradeHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Direction).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ExitReason).IsRequired().HasMaxLength(20);
            entity.Property(e => e.EntryPrice).HasPrecision(18, 2);
            entity.Property(e => e.ExitPrice).HasPrecision(18, 2);
            entity.Property(e => e.PnL).HasPrecision(18, 2);
            entity.Property(e => e.PnLPercent).HasPrecision(18, 4);
            entity.Property(e => e.OpenedAt).IsRequired();
            entity.Property(e => e.ClosedAt).IsRequired();

            // Foreign key relationship
            entity.HasOne(e => e.PaperTrade)
                .WithMany()
                .HasForeignKey(e => e.PaperTradeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Symbol);
            entity.HasIndex(e => e.IsWinner);
            entity.HasIndex(e => e.ClosedAt);
        });

        // Configure ReplayStateEntity (Singleton)
        modelBuilder.Entity<ReplayStateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Manually set to 1
            entity.Property(e => e.ReplayStartTime).IsRequired();
            entity.Property(e => e.Speed).IsRequired();
            entity.Property(e => e.IsRunning).IsRequired();
            entity.Property(e => e.Mode).IsRequired();
        });

        // Configure Account (Singleton)
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Manually set to 1 (single account for MVP)
            entity.Property(e => e.InitialBalance).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Balance).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Equity).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.AvailableCash).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure RiskSettings (Singleton)
        modelBuilder.Entity<RiskSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Manually set to 1
            entity.Property(e => e.DefaultRiskPercent).HasPrecision(5, 2).IsRequired();
            entity.Property(e => e.MaxRiskPercent).HasPrecision(5, 2).IsRequired();
            entity.Property(e => e.MinRiskRewardRatio).HasPrecision(5, 2).IsRequired();
            entity.Property(e => e.MaxCapitalPercent).HasPrecision(5, 2).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
}

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
    public DbSet<MarketModeEntity> MarketModes { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<RiskSettingsEntity> RiskSettings { get; set; }
    public DbSet<ScenarioConfigEntity> ScenarioConfigs { get; set; }
    public DbSet<SimulationSettingsEntity> SimulationSettings { get; set; }

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
            entity.Property(e => e.StopLoss).HasPrecision(18, 2);
            entity.Property(e => e.TakeProfit).HasPrecision(18, 2);
            entity.Property(e => e.PositionSize).HasPrecision(18, 4);
            entity.Property(e => e.InvestAmount).HasPrecision(18, 2);
            entity.Property(e => e.PnL).HasPrecision(18, 2);
            entity.Property(e => e.PnLPercent).HasPrecision(18, 4);
            entity.Property(e => e.OpenedAt).IsRequired();
            entity.Property(e => e.ClosedAt).IsRequired();

            entity.HasIndex(e => e.Symbol);
            entity.HasIndex(e => e.IsWinner);
            entity.HasIndex(e => e.ClosedAt);
        });

        // Configure MarketModeEntity (Singleton)
        modelBuilder.Entity<MarketModeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Manually set to 1
            entity.Property(e => e.Mode).IsRequired();
            entity.ToTable("MarketModes");
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

        // Configure ScenarioConfig (Singleton)
        modelBuilder.Entity<ScenarioConfigEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Manually set to 1
            entity.Property(e => e.ActivePreset).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConfigJson).IsRequired();
            entity.Property(e => e.IsEnabled).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure SimulationSettings (Singleton)
        modelBuilder.Entity<SimulationSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Manually set to 1
            entity.Property(e => e.VolatilityScale).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.DriftScale).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.MeanReversionStrength).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.FatTailMultiplier).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.FatTailMinSize).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.FatTailMaxSize).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.MaxReturnPerBar).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.LiveTickNoise).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.HighLowRangeMultiplier).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.PatternOverlayStrength).HasPrecision(5, 4).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Seed default watchlist with German DAX stocks
        var defaultWatchlist = new (string Symbol, string Company)[]
        {
            // DAX 40
            ("SAP.DE", "SAP"), ("SIE.DE", "Siemens"), ("ALV.DE", "Allianz"), ("BAS.DE", "BASF"),
            ("IFX.DE", "Infineon"), ("BMW.DE", "BMW"), ("MBG.DE", "Mercedes-Benz"), ("VOW3.DE", "Volkswagen"),
            ("DTE.DE", "Dt. Telekom"), ("RWE.DE", "RWE"), ("EOAN.DE", "E.ON"), ("MUV2.DE", "Munich Re"),
            ("CBK.DE", "Commerzbank"), ("DBK.DE", "Deutsche Bank"), ("ENR.DE", "Siemens Energy"),
            ("ADS.DE", "Adidas"), ("BAYN.DE", "Bayer"), ("HEI.DE", "Heidelberg Mat."), ("ZAL.DE", "Zalando"),
            ("DB1.DE", "Dt. Börse"), ("RHM.DE", "Rheinmetall"), ("MTX.DE", "MTU Aero"), ("AIR.DE", "Airbus"),
            ("SRT3.DE", "Sartorius"), ("SY1.DE", "Symrise"), ("HEN3.DE", "Henkel"), ("1COV.DE", "Covestro"),
            ("P911.DE", "Porsche AG"), ("VNA.DE", "Vonovia"), ("FRE.DE", "Fresenius"), ("HFG.DE", "HelloFresh"),
            ("DHER.DE", "Delivery Hero"), ("BEI.DE", "Beiersdorf"), ("HNR1.DE", "Hannover Rück"), ("BNR.DE", "Brenntag"),
            ("SHL.DE", "Siemens Health."), ("FME.DE", "Fresenius MC"), ("MRK.DE", "Merck KGaA"), ("QIA.DE", "Qiagen"),
            ("PAH3.DE", "Porsche SE"),
            // MDAX / Tech
            ("TMV.DE", "TeamViewer"), ("AIXA.DE", "Aixtron"), ("S92.DE", "SMA Solar"),
            ("EVT.DE", "Evotec"), ("AFX.DE", "Carl Zeiss"), ("NEM.DE", "Nemetschek"),
            ("WAF.DE", "Siltronic"), ("JEN.DE", "Jenoptik"), ("COK.DE", "Cancom"),
            ("GFT.DE", "GFT Tech"), ("NA9.DE", "Nagarro"), ("SMHN.DE", "Süss MicroTec"),
        };

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<WatchlistSymbol>().HasData(
            defaultWatchlist.Select((item, index) => new WatchlistSymbol
            {
                Id = index + 1,
                Symbol = item.Symbol,
                CompanyName = item.Company,
                CreatedAt = seedDate
            })
        );
    }
}

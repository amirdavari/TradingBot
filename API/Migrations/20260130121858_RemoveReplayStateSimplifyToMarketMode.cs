using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReplayStateSimplifyToMarketMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReplayStates");

            migrationBuilder.AddColumn<bool>(
                name: "AutoTradeEnabled",
                table: "RiskSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutoTradeMaxConcurrent",
                table: "RiskSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AutoTradeMinConfidence",
                table: "RiskSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AutoTradeRiskPercent",
                table: "RiskSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SelectedTimeframe",
                table: "RiskSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MarketModes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketModes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivePreset = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimulationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    VolatilityScale = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    DriftScale = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    MeanReversionStrength = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    FatTailMultiplier = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    FatTailMinSize = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    FatTailMaxSize = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    MaxReturnPerBar = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    LiveTickNoise = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    HighLowRangeMultiplier = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    PatternOverlayStrength = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "WatchlistSymbols",
                columns: new[] { "Id", "CompanyName", "CreatedAt", "Symbol" },
                values: new object[,]
                {
                    { 1, "SAP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SAP.DE" },
                    { 2, "Siemens", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SIE.DE" },
                    { 3, "Allianz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ALV.DE" },
                    { 4, "BASF", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BAS.DE" },
                    { 5, "Infineon", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "IFX.DE" },
                    { 6, "BMW", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BMW.DE" },
                    { 7, "Mercedes-Benz", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MBG.DE" },
                    { 8, "Volkswagen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VOW3.DE" },
                    { 9, "Dt. Telekom", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DTE.DE" },
                    { 10, "RWE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RWE.DE" },
                    { 11, "E.ON", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "EOAN.DE" },
                    { 12, "Munich Re", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MUV2.DE" },
                    { 13, "Commerzbank", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CBK.DE" },
                    { 14, "Deutsche Bank", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DBK.DE" },
                    { 15, "Siemens Energy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ENR.DE" },
                    { 16, "Adidas", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ADS.DE" },
                    { 17, "Bayer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BAYN.DE" },
                    { 18, "Heidelberg Mat.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HEI.DE" },
                    { 19, "Zalando", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ZAL.DE" },
                    { 20, "Dt. Börse", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DB1.DE" },
                    { 21, "Rheinmetall", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RHM.DE" },
                    { 22, "MTU Aero", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MTX.DE" },
                    { 23, "Airbus", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AIR.DE" },
                    { 24, "Sartorius", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SRT3.DE" },
                    { 25, "Symrise", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SY1.DE" },
                    { 26, "Henkel", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HEN3.DE" },
                    { 27, "Covestro", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1COV.DE" },
                    { 28, "Porsche AG", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "P911.DE" },
                    { 29, "Vonovia", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VNA.DE" },
                    { 30, "Fresenius", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "FRE.DE" },
                    { 31, "HelloFresh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HFG.DE" },
                    { 32, "Delivery Hero", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DHER.DE" },
                    { 33, "Beiersdorf", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BEI.DE" },
                    { 34, "Hannover Rück", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "HNR1.DE" },
                    { 35, "Brenntag", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BNR.DE" },
                    { 36, "Siemens Health.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SHL.DE" },
                    { 37, "Fresenius MC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "FME.DE" },
                    { 38, "Merck KGaA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MRK.DE" },
                    { 39, "Qiagen", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "QIA.DE" },
                    { 40, "Porsche SE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PAH3.DE" },
                    { 41, "TeamViewer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "TMV.DE" },
                    { 42, "Aixtron", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AIXA.DE" },
                    { 43, "SMA Solar", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "S92.DE" },
                    { 44, "Evotec", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "EVT.DE" },
                    { 45, "Carl Zeiss", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AFX.DE" },
                    { 46, "Nemetschek", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "NEM.DE" },
                    { 47, "Siltronic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "WAF.DE" },
                    { 48, "Jenoptik", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "JEN.DE" },
                    { 49, "Cancom", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "COK.DE" },
                    { 50, "GFT Tech", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "GFT.DE" },
                    { 51, "Nagarro", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "NA9.DE" },
                    { 52, "Süss MicroTec", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SMHN.DE" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketModes");

            migrationBuilder.DropTable(
                name: "ScenarioConfigs");

            migrationBuilder.DropTable(
                name: "SimulationSettings");

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "WatchlistSymbols",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DropColumn(
                name: "AutoTradeEnabled",
                table: "RiskSettings");

            migrationBuilder.DropColumn(
                name: "AutoTradeMaxConcurrent",
                table: "RiskSettings");

            migrationBuilder.DropColumn(
                name: "AutoTradeMinConfidence",
                table: "RiskSettings");

            migrationBuilder.DropColumn(
                name: "AutoTradeRiskPercent",
                table: "RiskSettings");

            migrationBuilder.DropColumn(
                name: "SelectedTimeframe",
                table: "RiskSettings");

            migrationBuilder.CreateTable(
                name: "ReplayStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRunning = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    ReplayStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Speed = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayStates", x => x.Id);
                });
        }
    }
}

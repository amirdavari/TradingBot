using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    InitialBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Equity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AvailableCash = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaperTrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Direction = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    EntryPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    StopLoss = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TakeProfit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionSize = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    InvestAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Confidence = table.Column<int>(type: "INTEGER", nullable: false),
                    Reasons = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ExitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    PnL = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    PnLPercent = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperTrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReplayStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ReplayStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Speed = table.Column<double>(type: "REAL", nullable: false),
                    IsRunning = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultRiskPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    MaxRiskPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    MinRiskRewardRatio = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    MaxCapitalPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistSymbols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistSymbols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PaperTradeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Direction = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    EntryPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ExitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    PnL = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PnLPercent = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    IsWinner = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExitReason = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Confidence = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeHistory_PaperTrades_PaperTradeId",
                        column: x => x.PaperTradeId,
                        principalTable: "PaperTrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaperTrades_OpenedAt",
                table: "PaperTrades",
                column: "OpenedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaperTrades_Status",
                table: "PaperTrades",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaperTrades_Symbol",
                table: "PaperTrades",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_ClosedAt",
                table: "TradeHistory",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_IsWinner",
                table: "TradeHistory",
                column: "IsWinner");

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_PaperTradeId",
                table: "TradeHistory",
                column: "PaperTradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_Symbol",
                table: "TradeHistory",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistSymbols_Symbol",
                table: "WatchlistSymbols",
                column: "Symbol",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "ReplayStates");

            migrationBuilder.DropTable(
                name: "RiskSettings");

            migrationBuilder.DropTable(
                name: "TradeHistory");

            migrationBuilder.DropTable(
                name: "WatchlistSymbols");

            migrationBuilder.DropTable(
                name: "PaperTrades");
        }
    }
}

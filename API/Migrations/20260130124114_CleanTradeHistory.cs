using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class CleanTradeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TradeHistory_PaperTrades_PaperTradeId",
                table: "TradeHistory");

            migrationBuilder.DropIndex(
                name: "IX_TradeHistory_PaperTradeId",
                table: "TradeHistory");

            migrationBuilder.DropColumn(
                name: "PaperTradeId",
                table: "TradeHistory");

            migrationBuilder.AddColumn<decimal>(
                name: "InvestAmount",
                table: "TradeHistory",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PositionSize",
                table: "TradeHistory",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StopLoss",
                table: "TradeHistory",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TakeProfit",
                table: "TradeHistory",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Migrate existing closed trades from PaperTrades to TradeHistory (before deleting them)
            migrationBuilder.Sql(@"
                INSERT INTO TradeHistory (Symbol, Direction, EntryPrice, ExitPrice, StopLoss, TakeProfit, Quantity, PositionSize, InvestAmount, PnL, PnLPercent, IsWinner, ExitReason, Confidence, DurationMinutes, OpenedAt, ClosedAt)
                SELECT 
                    Symbol, 
                    Direction, 
                    EntryPrice, 
                    COALESCE(ExitPrice, 0), 
                    StopLoss, 
                    TakeProfit, 
                    Quantity, 
                    PositionSize, 
                    InvestAmount, 
                    COALESCE(PnL, 0), 
                    COALESCE(PnLPercent, 0), 
                    CASE WHEN COALESCE(PnL, 0) > 0 THEN 1 ELSE 0 END, 
                    Status, 
                    Confidence, 
                    CAST((julianday(ClosedAt) - julianday(OpenedAt)) * 24 * 60 AS INTEGER), 
                    OpenedAt, 
                    ClosedAt
                FROM PaperTrades 
                WHERE Status != 'OPEN' AND ClosedAt IS NOT NULL
            ");

            // Delete closed trades from PaperTrades (keep only OPEN trades)
            migrationBuilder.Sql("DELETE FROM PaperTrades WHERE Status != 'OPEN'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvestAmount",
                table: "TradeHistory");

            migrationBuilder.DropColumn(
                name: "PositionSize",
                table: "TradeHistory");

            migrationBuilder.DropColumn(
                name: "StopLoss",
                table: "TradeHistory");

            migrationBuilder.DropColumn(
                name: "TakeProfit",
                table: "TradeHistory");

            migrationBuilder.AddColumn<int>(
                name: "PaperTradeId",
                table: "TradeHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_PaperTradeId",
                table: "TradeHistory",
                column: "PaperTradeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TradeHistory_PaperTrades_PaperTradeId",
                table: "TradeHistory",
                column: "PaperTradeId",
                principalTable: "PaperTrades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

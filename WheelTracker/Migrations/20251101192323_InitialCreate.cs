using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WheelTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ticker = table.Column<string>(type: "TEXT", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    CreditDebit = table.Column<string>(type: "TEXT", nullable: false),
                    Strategy = table.Column<int>(type: "INTEGER", nullable: true),
                    PriceAtOpen = table.Column<double>(type: "REAL", nullable: true),
                    Expiration = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Strike = table.Column<double>(type: "REAL", nullable: true),
                    Qty = table.Column<int>(type: "INTEGER", nullable: false),
                    Premium = table.Column<double>(type: "REAL", nullable: false),
                    Fees = table.Column<double>(type: "REAL", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    DTE = table.Column<int>(type: "INTEGER", nullable: true),
                    Moneyness = table.Column<double>(type: "REAL", nullable: true),
                    CloseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CloseTransType = table.Column<string>(type: "TEXT", nullable: true),
                    CloseQty = table.Column<int>(type: "INTEGER", nullable: true),
                    ClosePrice = table.Column<double>(type: "REAL", nullable: true),
                    CloseFee = table.Column<double>(type: "REAL", nullable: true),
                    DaysHeld = table.Column<int>(type: "INTEGER", nullable: true),
                    RealizedGainLoss = table.Column<double>(type: "REAL", nullable: true),
                    UnrealizedGainLoss = table.Column<double>(type: "REAL", nullable: true),
                    CurrentSharePrice = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");
        }
    }
}

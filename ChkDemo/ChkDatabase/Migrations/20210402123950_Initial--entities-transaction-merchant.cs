using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChkDatabase.Migrations
{
    public partial class Initialentitiestransactionmerchant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankSort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    APIKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardNameOnCard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardExpMonth = table.Column<int>(type: "int", nullable: false),
                    CardExpYear = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyID = table.Column<int>(type: "int", nullable: false),
                    CardCVV = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    DateTimeStateLastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StateMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Transactions_Merchants_MerchantID",
                        column: x => x.MerchantID,
                        principalTable: "Merchants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_MerchantID",
                table: "Transactions",
                column: "MerchantID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Merchants");
        }
    }
}

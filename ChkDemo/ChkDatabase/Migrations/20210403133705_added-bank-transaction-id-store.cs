using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChkDatabase.Migrations
{
    public partial class addedbanktransactionidstore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankTransactionID",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankTransactionID",
                table: "Transactions");
        }
    }
}

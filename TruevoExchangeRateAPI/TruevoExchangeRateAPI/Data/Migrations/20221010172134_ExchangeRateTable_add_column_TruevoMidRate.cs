using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruevoExchangeRateAPI.Data.Migrations
{
    public partial class ExchangeRateTable_add_column_TruevoMidRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TruevoMidRate",
                table: "ExchangeRates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TruevoMidRate",
                table: "ExchangeRates");
        }
    }
}

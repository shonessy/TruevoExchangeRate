using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TruevoExchangeRateAPI.Data.Migrations
{
    public partial class Exchange_Rate_Additional_Columns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellRate",
                table: "ExchangeRates",
                newName: "TruevoSellRate");

            migrationBuilder.RenameColumn(
                name: "MidRate",
                table: "ExchangeRates",
                newName: "TruevoBuyRate");

            migrationBuilder.RenameColumn(
                name: "BuyRate",
                table: "ExchangeRates",
                newName: "MastercardSellRate");

            migrationBuilder.AddColumn<decimal>(
                name: "MastercardBuyRate",
                table: "ExchangeRates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MastercardMidRate",
                table: "ExchangeRates",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MastercardBuyRate",
                table: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "MastercardMidRate",
                table: "ExchangeRates");

            migrationBuilder.RenameColumn(
                name: "TruevoSellRate",
                table: "ExchangeRates",
                newName: "SellRate");

            migrationBuilder.RenameColumn(
                name: "TruevoBuyRate",
                table: "ExchangeRates",
                newName: "MidRate");

            migrationBuilder.RenameColumn(
                name: "MastercardSellRate",
                table: "ExchangeRates",
                newName: "BuyRate");
        }
    }
}

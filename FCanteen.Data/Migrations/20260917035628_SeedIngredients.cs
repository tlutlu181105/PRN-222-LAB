using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FCanteen.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "Name", "StockQuantity", "Unit", "WarningThreshold" },
                values: new object[,]
                {
                    { 1, "Gạo", 200m, "kg", 20m },
                    { 2, "Thịt gà", 100m, "kg", 15m },
                    { 3, "Thịt heo", 100m, "kg", 15m },
                    { 4, "Thịt bò", 80m, "kg", 10m },
                    { 5, "Rau các loại", 150m, "kg", 20m },
                    { 6, "Trứng gà", 500m, "quả", 50m },
                    { 7, "Dầu ăn", 50m, "lít", 5m },
                    { 8, "Nước mắm", 30m, "lít", 5m },
                    { 9, "Đường", 40m, "kg", 5m },
                    { 10, "Bún/Mì/Phở khô", 100m, "kg", 15m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}

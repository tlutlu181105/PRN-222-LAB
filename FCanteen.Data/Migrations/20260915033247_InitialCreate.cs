using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FCanteen.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Protocol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CounterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderTicketId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLines_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketLines_OrderTickets_OrderTicketId",
                        column: x => x.OrderTicketId,
                        principalTable: "OrderTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Code", "IsAvailable", "Name", "Price", "Unit" },
                values: new object[,]
                {
                    { 1, "CM01", true, "Cơm gà xối mỡ", 35000m, "phần" },
                    { 2, "CM02", true, "Cơm sườn bì chả", 32000m, "phần" },
                    { 3, "CM03", true, "Cơm tấm sườn nướng", 30000m, "phần" },
                    { 4, "CM04", true, "Cơm chiên dương châu", 28000m, "phần" },
                    { 5, "CM05", true, "Cơm gà xé", 30000m, "phần" },
                    { 6, "MI01", true, "Mì xào bò", 27000m, "phần" },
                    { 7, "MI02", true, "Mì Quảng gà", 30000m, "phần" },
                    { 8, "PH01", true, "Phở bò tái", 32000m, "tô" },
                    { 9, "PH02", true, "Phở gà", 30000m, "tô" },
                    { 10, "BU01", true, "Bún chả cá", 28000m, "tô" },
                    { 11, "BU02", true, "Bún bò Huế", 30000m, "tô" },
                    { 12, "TR01", true, "Trà đá", 3000m, "ly" },
                    { 13, "TR02", true, "Trà tắc", 10000m, "ly" },
                    { 14, "NC01", true, "Nước cam ép", 15000m, "ly" },
                    { 15, "NS01", true, "Nước suối", 8000m, "chai" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_MenuItemId",
                table: "TicketLines",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_OrderTicketId",
                table: "TicketLines",
                column: "OrderTicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceLogs");

            migrationBuilder.DropTable(
                name: "TicketLines");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "OrderTickets");
        }
    }
}

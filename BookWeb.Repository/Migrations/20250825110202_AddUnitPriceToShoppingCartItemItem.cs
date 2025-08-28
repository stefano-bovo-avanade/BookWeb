using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWeb.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPriceToShoppingCartItemItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitPrice",
                table: "ShoppingCartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ShoppingCartItems");

        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWeb.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangedOrderToOrder : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
            name: "FK_OrderDetails_Orders_OrderId",
            table: "OrderDetails");

            migrationBuilder.RenameTable(
            name: "Orders",
            newName: "Orders");

            migrationBuilder.RenameColumn(
            name: "cartTotal",
            table: "ShoppingCarts",
            newName: "CartTotal");

            migrationBuilder.AddForeignKey(
            name: "FK_OrderDetails_Orders_OrderId",
            table: "OrderDetails",
            column: "OrderId",
            principalTable: "Orders",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
            name: "FK_OrderDetails_Orders_OrderId",
            table: "OrderDetails");

            migrationBuilder.RenameTable(
            name: "Orders",
            newName: "Orders");

            migrationBuilder.RenameColumn(
            name: "CartTotal",
            table: "ShoppingCarts",
            newName: "cartTotal");

            migrationBuilder.AddForeignKey(
            name: "FK_OrderDetails_Orders_OrderId",
            table: "OrderDetails",
            column: "OrderId",
            principalTable: "Orders",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        }

    }
}

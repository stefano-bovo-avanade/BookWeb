using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWeb.Service.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRoleDescriptionToName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Roles",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Roles",
                newName: "Description");
        }
    }
}
